rd /s /q out\build\win-x64
md out\build\win-x64
cd out\build\win-x64
cmake --config Release -G Ninja ..\..\..\
ninja
cd ..\..\..\