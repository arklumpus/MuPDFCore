#!/bin/bash

cwd=$PWD

if [[ "$OSTYPE" == "linux-gnu"* ]]; then
	architecture=$(uname -m);
	
	if [[ "$architecture" == "x86_64" ]]; then
		echo "Building linux-x64 native library..." ;
		echo ;
		rm -rf out/build/linux-x64 ;
		mkdir -p out/build/linux-x64 ;
		cd out/build/linux-x64 ;
		cmake -DLIBC="GLIBC" ../../../ ;
		make ;
	elif [[ "$architecture" == "aarch64" ]]; then
		echo "Building linux-arm64 native library..." ;
		echo ;
		rm -rf out/build/linux-arm64 ;
		mkdir -p out/build/linux-arm64 ;
		cd out/build/linux-arm64 ;
		cmake -DLIBC="GLIBC" ../../../ ;
		make ;
	elif [[ "$architecture" == "loongarch64" ]]; then
		echo "Building linux-loongarch64 native library..." ;
		echo ;
		rm -rf out/build/linux-loongarch64 ;
		mkdir -p out/build/linux-loongarch64 ;
		cd out/build/linux-loongarch64 ;
		cmake -DLIBC="GLIBC" -D ENABLE_PIC=ON -D BUILD_SHARED_LIBS=ON ../../../ ;
		make ;
	fi
elif [[ "$OSTYPE" == "linux-musl"* ]]; then
	architecture=$(uname -m);
	
	if [[ "$architecture" == "x86_64" ]]; then
		echo "Building linux-musl-x64 native library..." ;
		echo ;
		rm -rf out/build/linux-musl-x64 ;
		mkdir -p out/build/linux-musl-x64 ;
		cd out/build/linux-musl-x64 ;
		cmake -DLIBC="MUSL" ../../../ ;
		make ;
	elif [[ "$architecture" == "aarch64" ]]; then
		echo "Building linux-musl-arm64 native library..." ;
		echo ;
		rm -rf out/build/linux-musl-arm64 ;
		mkdir -p out/build/linux-musl-arm64 ;
		cd out/build/linux-musl-arm64 ;
		cmake -DLIBC="MUSL" ../../../ ;
		make ;
	elif [[ "$architecture" == "loongarch64" ]]; then
		echo "Building linux--musl-loongarch64 native library..." ;
		echo ;
		rm -rf out/build/linux-musl-loongarch64 ;
		mkdir -p out/build/linux-musl-loongarch64 ;
		cd out/build/linux-musl-loongarch64 ;
		cmake -DLIBC="MUSL" ../../../ ;
		make ;
	fi
elif [[ "$OSTYPE" == "darwin"* ]]; then
	architecture=$(uname -m);
	
	if [[ "$architecture" == "x86_64" ]]; then
        echo "Building mac-x64 native library..." ;
        echo ;
		rm -rf out/build/mac-x64 ;
		mkdir -p out/build/mac-x64
        cd out/build/mac-x64 ;
        cmake ../../../ ;
        make ;
    elif [[ "$architecture" == "arm64" ]]; then
        echo "Building mac-arm64 native library..." ;
        echo ;
		rm -rf out/build/mac-arm64 ;
		mkdir -p out/build/mac-arm64
        cd out/build/mac-arm64 ;
        cmake ../../../ ;
        make ;
    fi
fi

cd "$cwd"
