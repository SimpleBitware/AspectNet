# Contoso.AspectWeaver v1.2

Features:
- Logs entry, exit, parameters, and return values for methods.
- Handles auto-properties by synthesizing a private backing field and rewriting get/set bodies.

Usage:
1. Pack the NuGet package:
   dotnet pack -c Release
2. Add to your project:
   <PackageReference Include="Contoso.AspectWeaver" Version="1.2.0" />
3. Decorate members with [Log].
