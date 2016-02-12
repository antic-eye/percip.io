```

 ██▓███  ▓█████  ██▀███   ▄████▄   ██▓ ██▓███        ██▓ ▒█████
▓██░  ██▒▓█   ▀ ▓██ ▒ ██▒▒██▀ ▀█  ▓██▒▓██░  ██▒     ▓██▒▒██▒  ██▒
▓██░ ██▓▒▒███   ▓██ ░▄█ ▒▒▓█    ▄ ▒██▒▓██░ ██▓▒     ▒██▒▒██░  ██▒
▒██▄█▓▒ ▒▒▓█  ▄ ▒██▀▀█▄  ▒▓▓▄ ▄██▒░██░▒██▄█▓▒ ▒     ░██░▒██   ██░
▒██▒ ░  ░░▒████▒░██▓ ▒██▒▒ ▓███▀ ░░██░▒██▒ ░  ░ ██▓ ░██░░ ████▓▒░
▒▓▒░ ░  ░░░ ▒░ ░░ ▒▓ ░▒▓░░ ░▒ ▒  ░░▓  ▒▓▒░ ░  ░ ▒▓▒ ░▓  ░ ▒░▒░▒░
░▒ ░      ░ ░  ░  ░▒ ░ ▒░  ░  ▒    ▒ ░░▒ ░      ░▒   ▒ ░  ░ ▒ ▒░
░░          ░     ░░   ░ ░         ▒ ░░░        ░    ▒ ░░ ░ ░ ▒
            ░  ░   ░     ░ ░       ░             ░   ░      ░ ░
                         ░                       ░
```
[Percip.io](https://github.com/antic-eye/percip.io) - The working time logger

Use this tool to track your productivity. percip.io generates an
encrypted database file that contains timestamps and "in" or "out".

## Deployment

The tool does not need to be installed. Build it and copy the following files to
a directory of your choice:
```
Appccelerate.CommandLineParser.dll
Appccelerate.Fundamentals.dll
Microsoft.Win32.TaskScheduler.dll
percip.io.exe
```
Call `percip.io --init` from an elevated command prompt in this directory and 
percip.io will create Windows tasks for login, logout, session lock, session unlock.

You can check the tasks opening `taskschd.msc` searching for tasks prefixed with
*\_\_percip.io\_\_*.

## How it works

When you call percip.io with "lock" it tracks:
```
01.01.2016T08: 15 Max.Mustermann Out
```
When you call without args it tracks:
```
01.01.2016T08: 19 Max.Mustermann In
```
When you want to show your times, call it with `--query`.It will
read the db and calculate your working time beginning with the
first "in" per day, ending with the last "out".

Detailed help and usage information is available with `--help`.

## Automation

To automate the tracking, use `--init` and myLock will generate
Windows Scheduled tasks for screen lock/unlock and session
login/-out/reboot/shutdown. You will need administrative permissions for
the initialization task. Open an elevated command prompt.

## Disclaimer

I hacked this one for my needs, if you have special needs to reporting
and logging feel free to [drop an issue](https://github.com/antic-eye/percip.io/issues/new) or file a pull request or fork.
