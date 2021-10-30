rd /s /q out\build\win-x64
md out\build\win-x64
cd out\build\win-x64

cmake `-D CMAKE_BUILD_TYPE=Release` -G Ninja ..\..\..\

ninja
cd ..\..\..\