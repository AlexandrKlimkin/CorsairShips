# -*- coding: utf-8 -*-
from Tkinter import Tk
import sys

name_translation_separator = '⇨'

tk = Tk()
tk.withdraw()
clipboard = tk.clipboard_get()

if not isinstance(clipboard, str):
    print("There is no text in clipboard")
    raw_input("Press enter to exit")
    sys.exit(1)

print "Input:\n"
print clipboard
print

translation_map = {}
errors = ""
with open("nameTranslation.txt", 'r') as file:
    lines = file.readlines()
    for line in lines:
        if name_translation_separator in line:
            split = line.split(name_translation_separator)
            if len(split) != 2:
                errors += "Failed to split line " + line + "\n"
            translation_map[split[0]] = split[1].rstrip()

for key in translation_map:
    if key in clipboard:
        clipboard = clipboard.replace(key, translation_map[key])

tk.clipboard_clear()
tk.clipboard_append(clipboard)
tk.update()

print "Output:\n"
print clipboard
print

raw_input("Press enter to exit")
