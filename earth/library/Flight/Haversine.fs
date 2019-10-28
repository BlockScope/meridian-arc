﻿module Flight.Haversine

open System
open Flight.Units.Angle
open Flight.Units.DegMinSec
open Flight.LatLng
open Flight.Zone
open Flight.Geodesy
open Flight.Geodesy.Problem
open Flight.Earth.Sphere

let haversine (x : float<rad>) : float<rad> =
    let y = sin (float x / 2.0) in y * y * 1.0<rad>

let aOfHaversine {Lat = xLatF; Lng = xLngF} {Lat = yLatF; Lng = yLngF} =
    let (dLatF : float<rad>, dLngF : float<rad>) = (yLatF - xLatF, yLngF - xLngF)
    let hLatF = haversine dLatF
    let hLngF = haversine dLngF

    hLatF
    + cos (float xLatF)
    * cos (float yLatF)
    * hLngF

// Spherical distance using haversines and floating point numbers.
let distance (x : LatLng) (y : LatLng) : TaskDistance =
    let (Radius rEarth) = earthRadius
    let radDist : float<1> = 2.0 * asin (sqrt (float (aOfHaversine x y)))
    radDist * rEarth |> TaskDistance

let azimuthFwd' {Lat = xLat; Lng = xLng} {Lat = yLat; Lng = yLng} : float<rad> =
    let xLat' = float xLat
    let xLng' = float xLng
    let yLat' = float yLat
    let yLng' = float yLng

    let deltaLng = yLng' - xLng'
    let x = sin deltaLng * cos yLat'
    let y = cos xLat' * sin yLat' - sin xLat' * cos yLat' * cos deltaLng

    atan2 x y * 1.0<rad>

let azimuthFwd x y = azimuthFwd' x y |> Some

let azimuthRev x y =
    azimuthFwd y x
    |> Option.map (fun az -> let (Rad x) = Rad.Rotate (Math.PI * 1.0<rad> |> Rad) (Rad az) in x)

let rec inverse
    ({ x = {Lat = ``_Φ₁``; Lng = ``_L₁``} as x; y = {Lat = ``_Φ₂``; Lng = ``_L₂``} as y} : InverseProblem<LatLng>)
    : InverseSolution<TaskDistance,float<rad>> =
    { s = distance x y
    ; ``α₁`` = azimuthFwd' x y
    ; ``α₂`` = azimuthRev x y
    }

module SphereTests =
    open Xunit
    open Hedgehog

    [<Fact>]
    let ``haversines is not negative`` () = Property.check <| property {
            let! x = Gen.double <| Range.constantBounded ()
            return haversine (x * 1.0<rad>) >= 0.0<rad>
        }