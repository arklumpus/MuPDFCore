cmake_minimum_required (VERSION 3.8)

add_library(MuPDFWrapper SHARED "MuPDFWrapper.cpp" "MuPDFWrapper.h")

target_include_directories(MuPDFWrapper PRIVATE "include")
target_include_directories(MuPDFWrapper PRIVATE ".")

if (WIN32)
	target_link_libraries(MuPDFWrapper "${CMAKE_CURRENT_SOURCE_DIR}/lib/win-x64/libmupdf.lib")
endif()

if(UNIX AND NOT APPLE)
	target_link_libraries(MuPDFWrapper "${CMAKE_CURRENT_SOURCE_DIR}/lib/linux-x64/libmupdf.a")
	target_link_libraries(MuPDFWrapper "${CMAKE_CURRENT_SOURCE_DIR}/lib/linux-x64/libmupdf-third.a")
endif()

if (APPLE)
	target_link_libraries(MuPDFWrapper "${CMAKE_CURRENT_SOURCE_DIR}/lib/mac-x64/libmupdf.a")
	target_link_libraries(MuPDFWrapper "${CMAKE_CURRENT_SOURCE_DIR}/lib/mac-x64/libmupdf-third.a")
endif()