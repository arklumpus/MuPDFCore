cmake_minimum_required (VERSION 3.8)

add_library(MuPDFWrapper SHARED "MuPDFWrapper.cpp" "MuPDFWrapper.h")

target_include_directories(MuPDFWrapper PRIVATE "include")
target_include_directories(MuPDFWrapper PRIVATE ".")

set(TARGET "AMD64" CACHE STRING "Target build architecture for Windows.")

if (WIN32)
	if (${TARGET} MATCHES "AMD64")
		target_link_libraries(MuPDFWrapper "${CMAKE_CURRENT_SOURCE_DIR}/lib/win-x64/libmupdf.lib")
	endif()
	if (${TARGET} MATCHES "x86")
		target_link_libraries(MuPDFWrapper "${CMAKE_CURRENT_SOURCE_DIR}/lib/win-x86/libmupdf.lib")
	endif()
	if (${TARGET} MATCHES "ARM64")
		target_link_libraries(MuPDFWrapper "${CMAKE_CURRENT_SOURCE_DIR}/lib/win-arm64/libmupdf.lib")
	endif()
endif()

if(UNIX AND NOT APPLE)
	target_link_libraries(MuPDFWrapper "${CMAKE_CURRENT_SOURCE_DIR}/lib/linux-x64/libmupdf.a")
	target_link_libraries(MuPDFWrapper "${CMAKE_CURRENT_SOURCE_DIR}/lib/linux-x64/libmupdf-third.a")
endif()

if (APPLE)
	if (${CMAKE_HOST_SYSTEM_PROCESSOR} MATCHES "x86_64")
		target_link_libraries(MuPDFWrapper "${CMAKE_CURRENT_SOURCE_DIR}/lib/mac-x64/libmupdf.a")
		target_link_libraries(MuPDFWrapper "${CMAKE_CURRENT_SOURCE_DIR}/lib/mac-x64/libmupdf-third.a")
	endif()
	
	if (${CMAKE_HOST_SYSTEM_PROCESSOR} MATCHES "arm64")
		target_link_libraries(MuPDFWrapper "${CMAKE_CURRENT_SOURCE_DIR}/lib/mac-arm64/libmupdf.a")
		target_link_libraries(MuPDFWrapper "${CMAKE_CURRENT_SOURCE_DIR}/lib/mac-arm64/libmupdf-third.a")
	endif()
endif()