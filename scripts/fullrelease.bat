rm out -r
md out

cd ..

dotnet clean

cd src\core
dotnet restore --no-cache -f
dotnet publish -c Release

copy bin\release\Cosm.Net.Core.* ..\..\scripts\out\ /Y

cd ..\..\

cd src\Cosm.Net
dotnet restore --no-cache -f
dotnet publish -c Release

copy bin\release\Cosm.Net.* ..\..\scripts\out\ /Y

cd ..\..\

cd gen\common
dotnet restore --no-cache -f
dotnet publish -c Release

copy bin\release\Cosm.Net.Generators.Common.* ..\..\scripts\out\ /Y

cd ..\..\

cd gen\proto-module
dotnet restore --no-cache -f
dotnet publish -c Release

copy bin\release\Cosm.Net.Generators.Proto.* ..\..\scripts\out\ /Y

cd ..\..\

cd src\modules\cosmos-sdk
dotnet restore --no-cache -f
dotnet publish -c Release

copy bin\release\Cosm.Net.CosmosSdk.* ..\..\..\scripts\out\ /Y

cd ..\..\..\

cd src\modules\osmosis
dotnet restore --no-cache -f
dotnet publish -c Release

copy bin\release\Cosm.Net.Osmosis.* ..\..\..\scripts\out\ /Y

cd ..\..\..\

cd src\modules\wasm
dotnet clean
dotnet restore --no-cache -f
dotnet publish -c Release

copy bin\release\Cosm.Net.Wasm.* ..\..\..\scripts\out\ /Y

cd ..\..\..\

cd gen\wasm-schema
dotnet restore --no-cache -f
dotnet publish -c Release

copy bin\release\Cosm.Net.Generators.CosmWasm.* ..\..\scripts\out\ /Y

cd ..\..\
cd scripts