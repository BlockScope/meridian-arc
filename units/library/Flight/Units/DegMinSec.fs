﻿module Flight.Units.DegMinSec

open System
open FSharp.Data.UnitSystems.SI.UnitSymbols
open Flight.Units.Angle
open Flight.Units.Convert

type DmsTuple = int<deg> * int<min> * float<s>

let sign' (x : float) : int = sign x |> function | 0 -> 1 | y -> y

/// If any of degree, minute or second are negative then -1 otherwise +1.
let signDMS (d : int<deg>, m : int<min>, s : float<s>) : int =
    if List.map sign [float d; float m; float s] |> List.contains (-1) then -1 else 1

let toDeg ((d : int<deg>, m : int<min>, s : float<s>) as dms) : float<deg> =
    let deg = float (abs d) * 1.0<deg>
    let min = float (abs m) * 1.0<min>
    let sec = abs s
    float (signDMS dms) * (deg + convertMinToDeg min + convertSecToDeg sec)

let toRad : DmsTuple -> float<rad> = toDeg >> convertDegToRad

let fromDeg (d : float<deg>) : DmsTuple =
    let dAbs : float<deg> = abs d
    let dd : int<deg> = int dAbs * 1<deg>
    let dFrac : float<deg> = dAbs - float dd * 1.0<deg>
    let minsFrac : float<min> = convertDegToMin dFrac
    let mins : float<min> = floor (float minsFrac) * 1.0<min>
    let secs : float<s> = convertMinToSec (minsFrac - mins)
    (sign' (float d) * dd, int mins * 1<min>, secs)

let fromRad : float<rad> -> DmsTuple = convertRadToDeg >> fromDeg

let normalizeDeg (degPlusMinus : float<deg>) : float<deg> =
    let deg = abs degPlusMinus
    let n = truncate (float deg / 360.0)
    let d = deg - float n * 360.0<deg>

    if d = 0.0<deg> then d
    elif degPlusMinus > 0.0<deg> then d
    else -d + 360.0<deg>

let normalizeRad : float<rad> -> float<rad> = convertRadToDeg >> normalizeDeg >> convertDegToRad
let normalizeDms : DmsTuple -> DmsTuple = toDeg >> normalizeDeg >> fromDeg

let plusMinusPiDeg (degPlusMinus : float<deg>) : float<deg> =
    if Double.IsNaN (float degPlusMinus) then degPlusMinus else
    let isEven x = x % 2 = 0
    let deg = abs degPlusMinus
    let n = truncate (float deg / 180.0)
    let d = deg - float n * 180.0<deg>
    let m = float (sign degPlusMinus)
    match (n, d) with
    | (a, 0.0<deg>) ->
        if isEven (int a) then 0.0<deg> else m * 180.0<deg>
    | (a, b) ->
        (m * b) +
            if isEven (int a) then 0.0<deg>
            elif degPlusMinus >= 0.0<deg> then -180.0<deg>
            else 180.0<deg>

let plusMinusHalfPiDeg : float<deg> -> float<deg> option =
    plusMinusPiDeg >> fun x -> if x < -90.0<deg> || x > 90.0<deg> then None else Some x

let plusMinusPiRad : float<rad> -> float<rad> = convertRadToDeg >> plusMinusPiDeg >> convertDegToRad
let plusMinusHalfPiRad : float<rad> -> float<rad> option = convertRadToDeg >> plusMinusHalfPiDeg >> Option.map convertDegToRad 

let plusMinusPiDms : DmsTuple -> DmsTuple = toDeg >> plusMinusPiDeg >> fromDeg
let plusMinusHalfPiDms : DmsTuple -> DmsTuple option = toDeg >> plusMinusHalfPiDeg >> Option.map fromDeg

let rotateDeg (rotation : float<deg>) (deg : float<deg>) : float<deg> = deg + rotation |> normalizeDeg
let rotateRad (rotation : float<rad>) (r : float<rad>) : float<rad> = r + rotation |> normalizeRad

let rotateDms (rotation : DmsTuple) (deg : DmsTuple) : DmsTuple =
    rotateDeg (toDeg rotation) (toDeg deg) |> fromDeg

type Rad =
    | Rad of float<rad>

    /// 0 <= a <= 2π
    static member Normalize (Rad r) : Rad = normalizeRad r |> Rad 

    /// -π <= a <= +π
    static member PlusMinusPi (Rad r) : Rad = normalizeRad r |> plusMinusPiRad |> Rad
    /// -π/2 <= a <= +π/2
    static member PlusMinusHalfPi (Rad r) : Rad option = plusMinusHalfPiRad r |> Option.map Rad 
    static member Rotate (Rad rotation) (Rad r) : Rad = rotateRad rotation r |> Rad

    static member ToRad (Rad r) : float<rad> = r
    static member FromRad (r : float<rad>) : Rad = Rad r

    static member ToDeg (Rad r) : float<deg> = convertRadToDeg r
    static member FromDeg (d : float<deg>) : Rad = convertDegToRad d |> Rad

    override x.ToString() = let (Rad y) = x in String.Format("{0:R} rad", y)

type Deg =
    | Deg of float<deg>

    /// 0 <= a <= 2π
    static member Normalize (Deg d) : Deg = normalizeDeg d |> Deg

    /// -π <= a <= +π
    static member PlusMinusPi (Deg d) : Deg = plusMinusPiDeg d |> Deg
    /// -π/2 <= a <= +π/2
    static member PlusMinusHalfPi (Deg d) : Deg option = plusMinusHalfPiDeg d |> Option.map Deg
    static member Rotate (Deg rotation) (Deg d) : Deg = rotateDeg rotation d |> Deg

    static member ToRad (Deg d) : float<rad> = convertDegToRad d
    static member FromRad (r : float<rad>) : Deg = convertRadToDeg r |> Deg

    static member ToDeg (Deg d) : float<deg> = d
    static member FromDeg (d : float<deg>) : Deg = Deg d

    override x.ToString() = let (Deg y) = x in String.Format("{0:R}°", y)

type DMS =
    | DMS of DmsTuple

    /// 0 <= a <= 2π
    static member Normalize (DMS dms) : DMS = normalizeDms dms |> DMS

    /// -π <= a <= +π
    static member PlusMinusPi (DMS dms) : DMS = plusMinusPiDms dms |> DMS
    /// -π/2 <= a <= +π/2
    static member PlusMinusHalfPi (DMS d) : DMS option = plusMinusHalfPiDms d |> Option.map DMS

    static member Rotate (DMS rotation) (DMS d) : DMS = rotateDms rotation d |> DMS

    static member FromTuple (d : int, m : int, s : float) : DMS = DMS (d * 1<deg>, m * 1<min>, s * 1.0<s>)
    static member ToRad (DMS dms) : float<rad> = toRad dms
    static member FromRad (r : float<rad>) : DMS = fromRad r |> DMS

    static member ToDeg (DMS dms) : float<deg> = toDeg dms
    static member FromDeg (d : float<deg>) : DMS = fromDeg d |> DMS

    member x.Sign : int =
        match x with
        | DMS dms -> signDMS dms

    override x.ToString() =
        let signSymbolDMS (dms : DMS) : String =
            if x.Sign < 0 then "-" else ""

        let secToShow (sec : float<s>) : String =
            let isec = Math.Floor (float sec)
            if isec = float sec
                // NOTE: Use String.Format instead of sprintf because it can do
                // roundtrip formatting of floating point.
                then String.Format("{0:R}", abs (int isec))
                else String.Format("{0:R}", abs sec)

        let secToShow' (sec : float<s>) : String =
            let isec = Math.Floor (float sec)
            if isec = float sec
                then String.Format("{0:R}", abs (int isec))
                else sprintf "%.6f" (abs sec)

        match x with
        | DMS (deg, 0<min>, 0.0<s>) -> string deg + "°"
        | DMS (0<deg>, 0<min>, sec) -> secToShow' sec + "''"
        | DMS (deg, min, 0.0<s>) as dms ->
            signSymbolDMS dms
            + string (abs deg)
            + "°"
            + string (abs min)
            + "'"
        | DMS (0<deg>, min, sec) as dms ->
            signSymbolDMS dms
            + string (abs min)
            + "'"
            + secToShow sec
            + "''"
        | DMS (deg, min, sec) as dms ->
            signSymbolDMS dms
            + string (abs deg)
            + "°"
            + string (abs min)
            + "'"
            + secToShow sec
            + "''"

and DiffDMS = DMS -> DMS -> DMS

module DegMinSecTests =
    open Xunit
    open Swensen.Unquote
    open Hedgehog

    [<Theory>]
    [<InlineData(0, 0, 0.0, 0.0)>]
    [<InlineData(1.0, 0, 0.0, 1.0)>]
    [<InlineData(289, 30, 0.0, 289.5)>]
    let ``convert dms to deg`` deg min sec deg' =
        test <@ DMS (deg, min, sec) |> DMS.ToDeg = deg' * 1.0<deg> @>

    [<Theory>]
    [<InlineData(0, 0, 0.0, 0.0)>]
    [<InlineData(289, 30, 0.0, 289.5)>]
    let ``(d, m, s) to deg`` d m s deg = test <@ toDeg (d, m, s) = deg @>

    [<Theory>]
    [<InlineData(0.0, 0, 0, 0.0)>]
    [<InlineData(1.0, 1, 0, 0.0)>]
    [<InlineData(289.5, 289, 30, 0.0)>]
    let ``from deg to (d, m, s)`` deg d m s = test <@ fromDeg deg = (d, m, s) @>

    [<Theory>]
    [<InlineData(0.0, "0°")>]
    [<InlineData(360.0, "0°")>]
    [<InlineData(-360.0, "0°")>]
    [<InlineData(720.0, "0°")>]
    [<InlineData(-720.0, "0°")>]
    [<InlineData(1.0, "1°")>]
    [<InlineData(-1.0, "359°")>]
    [<InlineData(180.0, "180°")>]
    [<InlineData(-180.0, "180°")>]
    [<InlineData(-190.0, "170°")>]
    [<InlineData(-170.0, "190°")>]
    [<InlineData(-190.93544548, "169.06455452°")>]
    [<InlineData(-169.06455452, "190.93544548°")>]
    let ``show normalize deg`` deg s = test <@ Deg (deg * 1.0<deg>) |> Deg.Normalize |> string = s @>

    [<Theory>]
    [<InlineData(0.0, "0°")>]
    [<InlineData(90.0, "90°")>]
    [<InlineData(-90.0, "-90°")>]
    [<InlineData(180.0, "180°")>]
    [<InlineData(-180.0, "-180°")>]
    [<InlineData(181.0, "-179°")>]
    [<InlineData(-181.0, "179°")>]
    let ``show (-π, +π) deg`` deg s = test <@ Deg (deg * 1.0<deg>) |> Deg.PlusMinusPi |> string = s @>

    [<Theory>]
    [<InlineData(0.0, "Some(0°)")>]
    [<InlineData(90.0, "Some(90°)")>]
    [<InlineData(-90.0, "Some(-90°)")>]
    [<InlineData(91.0, "")>]
    [<InlineData(-91.0, "")>]
    let ``show (-π/2, +π/2) deg`` deg s = test <@ Deg (deg * 1.0<deg>) |> Deg.PlusMinusHalfPi |> string = s @>

    [<Theory>]
    [<InlineData(0.0, "0°")>]
    [<InlineData(1.0, "1°")>]
    [<InlineData(-1.0, "-1°")>]
    [<InlineData(169.06666666622118, "169°3'59.99999839625161''")>]
    [<InlineData(-169.06666666622118, "-169°3'59.99999839625161''")>]
    let ``show from deg to dms`` deg s = test <@ DMS.FromDeg deg |> string = s @>

    [<Theory>]
    [<InlineData(0.0, "0°")>]
    [<InlineData(180.0, "180°")>]
    [<InlineData(1.0, "1°")>]
    [<InlineData(-180.0, "180°")>]
    [<InlineData(-1.0, "359°")>]
    [<InlineData(190.93333333377882, "190°56'1.6037483874242753E-06''")>]
    let ``show normalize deg to dms`` deg s = test <@ DMS.FromDeg deg |> DMS.Normalize |> string = s @>

    [<Theory>]
    [<InlineData(0, 0, 0.0, "0°")>]
    [<InlineData(180, 0, 0.0, "180°")>]
    [<InlineData(1, 0, 0.0, "1°")>]
    [<InlineData(-180, 0, 0.0, "180°")>]
    [<InlineData(-1, 0, 0.0, "359°")>]
    [<InlineData(190, 56, 1.6037483874242753e-6, "190°56'1.6037483874242753E-06''")>]
    let ``show normalize dms`` deg min sec s =
        test <@ DMS (deg, min, sec) |> DMS.Normalize |> string = s @>

    [<Theory>]
    [<InlineData(0, 0, "0°")>]
    [<InlineData(180, 0, "180°")>]
    [<InlineData(0, 180, "180°")>]
    [<InlineData(-180, 0, "180°")>]
    [<InlineData(0, -180, "180°")>]
    [<InlineData(-360, 0, "0°")>]
    [<InlineData(0, -360, "0°")>]
    let ``show rotate deg`` rotation deg s =
        test <@ rotateDeg (rotation * 1.0<deg>) (deg * 1.0<deg>) |> Deg |> string = s @>

    [<Fact>]
    let ``deg via DMS`` () = Property.check <| property {
            let! d = Gen.int <| Range.constantBounded ()
            return (toDeg (d * 1<deg>, 0<min>, 0.0<s>)) = float d * 1.0<deg>
        }

    [<Fact>]
    let ``min via DMS`` () = Property.check <| property {
            let! m = Gen.int <| Range.constantBounded ()
            return (toDeg (0<deg>, m * 1<min>, 0.0<s>)) = convertMinToDeg (float m * 1.0<min>)
        }

    [<Fact>]
    let ``sec via DMS`` () = Property.check <| property {
            let! sec = Gen.double <| Range.constantBounded ()
            return (toDeg (0<deg>, 0<min>, sec * 1.0<s>)) = convertSecToDeg (sec * 1.0<s>)
        }

    [<Fact>]
    let ``deg min via DMS`` () = Property.check <| property {
            let! d = Gen.int <| Range.constantBounded ()
            let! m = Gen.int <| Range.constantBounded ()
            let dms = (d * 1<deg>, m * 1<min>, 0.0<s>)
            let plusMinus = float (signDMS dms)
            let deg = float (abs d) * 1.0<deg>
            let min = float (abs m) * 1.0<min>
            return (toDeg dms) = plusMinus * (deg + convertMinToDeg min)
        }

    [<Fact>]
    let ``deg sec via DMS`` () = Property.check <| property {
            let! d = Gen.int <| Range.constantBounded ()
            let! s = Gen.double <| Range.constantBounded ()
            let dms = (d * 1<deg>, 0<min>, s * 1.0<s>)
            let plusMinus = float (signDMS dms)
            let deg = float (abs d) * 1.0<deg>
            let sec = abs s * 1.0<s>
            return (toDeg dms) = plusMinus * (deg + convertSecToDeg sec)
        }

    [<Fact>]
    let ``min sec via DMS`` () = Property.check <| property {
            let! m = Gen.int <| Range.constantBounded ()
            let! s = Gen.double <| Range.constantBounded ()
            let dms = (0<deg>, m * 1<min>, s * 1.0<s>)
            let plusMinus = float (signDMS dms)
            let sec = abs s * 1.0<s>
            let min = float (abs m) * 1.0<min>
            return (toDeg dms) = plusMinus * (convertMinToDeg min + convertSecToDeg sec)
        }

    [<Fact>]
    let ``deg min sec via DMS`` () = Property.check <| property {
            let! d = Gen.int <| Range.constantBounded ()
            let! m = Gen.int <| Range.constantBounded ()
            let! s = Gen.double <| Range.constantBounded ()
            let dms = (d * 1<deg>, m * 1<min>, s * 1.0<s>)
            let deg = float (abs d) * 1.0<deg>
            let min = float (abs m) * 1.0<min>
            let sec = abs s * 1.0<s>
            let plusMinus = float (signDMS dms)
            return (toDeg dms) = plusMinus * (deg + convertMinToDeg min + convertSecToDeg sec)
        }
