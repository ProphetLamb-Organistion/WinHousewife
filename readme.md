# About
IntegratedCalculator is a calculator app that aims to be more accessible then the Windows Calculator or Cortana queries, while using a command line like input.

To solve expressions this applications uses the [mXpaser libary](https://github.com/mariuszgromada/MathParser.org-mXparser) Copyright 2010 - 2020 Mariusz Gromada licenced under Simplified BSD Licence.

# Motivation
I was fedup with windows search evaluating expressions using a browser request, and descided that there should be a integrated solution, that can evaluate expressions. 
So I picked up a nice expression parser and implemented a simple CLI.
That done I registered an easy to access hotkey that was unused [Win]+[C] (cause ain't nobody using Cortana). Later I added a few more accessibility features and allowed the user to configure them using a settings.json.

# Features

## Hotkeys
* [Win]+[C] - Opens this window
* [Esc] - Hides this window
* [Cntr]+[W] - Closes the webbrowser, if open.

## Native commands
Native commands are commands that are hardcoded into the app, by well me. Here is a list of them:
* `help`: Displays a general or specified help text for commands.
Type `command help` or `help command` to obtain additional help information for the specified command.
Type `help math` for the help text of the math engine.
* `size`: Sets the size of the window to a predefined or specified value.
e.g. `size small`, `size 600,800`. Sizes can be predefined in settings.json.
* `clear`: Clears the screen.
* `exit`: Exits the application entirely.
* `close`: Minimizes the application to the background.
* `cpy`: Copies the last result or result of the expression to the clipboard. e.g. `2+2` `cpy` copies 4, `cpy 2+2` copies 4, `cpy e0` copies the result of the 1st expression.
* `settings`: Allows limited modification of applications settings. It is adviced to edit settings.json directly instead.

## Web-queries
Queries a searchterm on a specific webpage in the integrated cef-browser.
**Example**
`web hello world!` opens the searchterm "hello world!" on the Ecosia searchengine by default. But that all can be configured in settings.json to just about anything.
```json
"WebCommands": [
  {
    "Cmdlet": "web",
    "Helptext": "Queries the searchterm on Ecosia, in the integrated webbrowser.",
    "Documentation": "The uri of the site is ecosia.org",
    "UriFormat": "https://www.ecosia.org/search?q={0}"
  }
]
```

## Launcher
Lauches a program using `Process.Start` with the argument list passed. Launch commands can also just point to a file or folder that will be opened using the associated default application.
**Example**
`npp "C:\some\file.txt"` opens the file specified in Notepad++.
These commands can also be altered to whatever the user desires.
```json
"LaunchCommands": [
  {
    "Cmdlet": "npp",
    "Helptext": "Launches Notepad++.",
    "Target": "C:\\Program Files (x86)\\Notepad++\\notepad++.exe"
  }
],
```
# External sources
Icons made by <a href="https://www.flaticon.com/authors/freepik" title="Freepik">Freepik</a> from <a href="https://www.flaticon.com/" title="Flaticon"> www.flaticon.com</a>