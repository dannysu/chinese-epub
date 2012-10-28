#!/bin/sh

if [ $# -ne 4 ]; then
    echo $0 "input_file book_title output_directory line_count"
    exit
fi

mkdir "$3"
iconv -f GBK -t UTF-16 -o "$3/source.txt" -c "$1"
mono convert.exe "$3/source.txt" "$2" "$3" "$4"
rm "$3/source.txt"

cd "$3"
zip -r "$2.epub" ./*
