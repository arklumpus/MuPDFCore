set platform=unknown

if "%PROCESSOR_ARCHITECTURE%" == "x86" (set platform=x86)
if "%PROCESSOR_ARCHITECTURE%" == "AMD64" (set platform=x64)
if "%PROCESSOR_ARCHITECTURE%" == "ARM64" (set platform=arm64)

echo Building for %platform%

rd /s /q out\build\win-%platform%
md out\build\win-%platform%
cd out\build\win-%platform%

cmake -D CMAKE_BUILD_TYPE=Release -D TARGET:STRING=%PROCESSOR_ARCHITECTURE% -G Ninja ..\..\..\

ninja
cd ..\..\..\
