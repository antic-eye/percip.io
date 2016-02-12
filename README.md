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

To automate the tracking, use `--init` and myLock will generate
Windows Scheduled tasks for screen lock/unlock and session
login/-out/reboot/shutdown. You will need administrative permissions for
the initialization task. Open an elevated command prompt.

I hacked this one for my needs, if you have special needs to reporting
and logging feel free to [drop an issue](https://github.com/antic-eye/percip.io/issues/new) or file a pull request or fork.
