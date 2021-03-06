﻿module rec Flight.Geodesy

open Flight.Zone
open Flight.Earth.Ellipsoid
open Flight.Geodesy.Math
open Flight.Geodesy.Problem

module Math =
    open Flight.Units.Angle
    open Flight.Units.DegMinSec

    type EarthMath
        = Haversines
        | Vincenty

    type EarthModel
        = EarthAsSphere of Radius
        | EarthAsEllipsoid of Ellipsoid

    let normalizeLng (lng : float<rad>) : float<rad> =
        Rad lng |> Rad.PlusMinusPi |> Rad.ToRad

module Problem =
    /// <summary>The inputs for the direct or forward problem in geodesy.</summary>
    /// <param name="x">The departure point on the ellipsoid.</param>
    /// <param name="α₁">The azimuth from the departure point.</param>
    /// <param name="s">The distance to the arrival point.</param>
    type DirectProblem<'a, 'α, 's>  = {x : 'a; ``α₁`` : 'α; s : 's}

    /// <summary> The inputs for the inverse or reverse problem in geodesy.</summary>
    /// <param name="x">The departure point.</param>
    /// <param name="y">The arrival point.</param>
    type InverseProblem<'a> = {x : 'a; y : 'a}

    /// <summary>The outputs for the solution to the direct or forward problem in geodesy.</summary>
    /// <param name="y">The arrival point.</param>
    /// <param name="α₂">The azimuth at the arrival point.</param>
    type DirectSolution<'a, 'α> = {y : 'a; ``α₂`` : 'α option}

    /// <summary>The outputs for the solution to the inverse or reverse problem in geodesy.</summary>
    /// <param name="s">The distance between the departure and arrival points.</param>
    /// <param name="α₁">The azimuth at the departure point.</param>
    /// <param name="α₂">The azimuth at the arrival point.</param>
    type InverseSolution<'s, 'α> = {s : 's; ``α₁`` : 'α; ``α₂`` : 'α option}

    type IGeodesyProblems =
        abstract member Direct<'a, 'α, 's>
            : InverseProblem<'a>
            -> InverseSolution<'s, 'α>
            -> Option<DirectProblem<'a, 'α, 's> * DirectSolution<'a, 'α>>

        abstract member Inverse<'a, 'α, 's>
            : DirectProblem<'a, 'α, 's>
            -> DirectSolution<'a, 'α>
            -> Option<InverseProblem<'a> * InverseSolution<'s, 'α>>