# Meridian Arc

Geodesy problems and their solutions.

![dotnet-win](https://github.com/BlockScope/meridian-arc/workflows/dotnet-win/badge.svg)
![dotnet-ubuntu](https://github.com/BlockScope/meridian-arc/workflows/dotnet-ubuntu/badge.svg)
![dotnet-mac](https://github.com/BlockScope/meridian-arc/workflows/dotnet-mac/badge.svg)

## Building

```
> dotnet --version
3.1.202
> donet clean
> dotnet build
```

## Testing

```
> dotnet test units
Test run for .../meridian-arc/units/bin/Debug/netcoreapp3.0/flight-units.dll(.NETCoreApp,Version=v3.0)
Microsoft (R) Test Execution Command Line Tool Version 16.5.0
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...

A total of 1 test files matched the specified pattern.

Test Run Successful.
Total tests: 64
     Passed: 64

> dotnet test earth
Test run for .../meridian-arc/earth/bin/Debug/netcoreapp3.0/flight-earth.dll(.NETCoreApp,Version=v3.0)
Microsoft (R) Test Execution Command Line Tool Version 16.5.0
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...

A total of 1 test files matched the specified pattern.

Test Run Successful.
Total tests: 202
     Passed: 202
```

## Packaging

```
> dotnet tool restore
Tool 'paket' (version '5.226.0') was restored. Available commands: paket

Restore was successful.

> dotnet paket pack paket-pack
Paket version 5.226.0
Packaging: .../meridian-arc/paket.template
Wrote: .../meridian-arc/paket-pack/meridian-arc.0.1.3.nupkg
```

## Nuget Package Test

```
> dotnet run --project test-earth
Distances from Vincenty's 1975 paper using 1x Bessel and 4x Hayford ellipsoids:
{ equatorialR = Radius 6377397.155
  recipF = 299.1528128 }
{ equatorialR = Radius 6378388.0
  recipF = 297.0 }
Distance ((DMS (55, 45, 0.0), DMS (0, 0, 0.0)), (DMS (-33, 26, 0.0), DMS (108, 13, 0.0))) = 14110526.169596m
Distance ((DMS (37, 19, 54.95367), DMS (0, 0, 0.0)), (DMS (26, 7, 42.83946), DMS (41, 28, 35.50729))) = 4085966.702613m
Distance ((DMS (35, 16, 11.24862), DMS (0, 0, 0.0)), (DMS (67, 22, 14.77638), DMS (137, 47, 28.31435))) = 8084823.838297m
Distance ((DMS (1, 0, 0.0), DMS (0, 0, 0.0)), (DMS (0, -59, 53.83076), DMS (179, 17, 48.02997))) = 19959999.999803m
Distance ((DMS (1, 0, 0.0), DMS (0, 0, 0.0)), (DMS (1, 1, 15.18952), DMS (179, 46, 17.84244))) = 19780006.558786m
```

## License

```
Copyright © Phil de Joux 2017-2019
Copyright © Block Scope Limited 2017-2019
```

This software is subject to the terms of the Mozilla Public License, v2.0. If
a copy of the MPL was not distributed with this file, you can obtain one at
http://mozilla.org/MPL/2.0/.
