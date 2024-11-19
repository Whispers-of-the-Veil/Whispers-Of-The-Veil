#!/bin/bash
if [ -z "$1" ]; then
    echo "Please provide a directory path."
    exit 1
fi

find "$1" -type f -name '*.flac' | while read -r file; do
    flac --decode --stdout "$file" > "${file%.flac}.wav" && rm "$file"
done