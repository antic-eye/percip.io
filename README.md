
 ███▄ ▄███▓▓██   ██▓ ██▓     ▒█████   ▄████▄   ██ ▄█▀
▓██▒▀█▀ ██▒ ▒██  ██▒▓██▒    ▒██▒  ██▒▒██▀ ▀█   ██▄█▒
▓██    ▓██░  ▒██ ██░▒██░    ▒██░  ██▒▒▓█    ▄ ▓███▄░
▒██    ▒██   ░ ███▓░▒██░    ▒██   ██░▒▓▓▄ ▄██▒▓██ █▄
▒██▒   ░██▒  ░ ██▒▓░░██████▒░ ████▓▒░▒ ▓███▀ ░▒██▒ █▄
░ ▒░   ░  ░   ██▒▒▒ ░ ▒░▓  ░░ ▒░▒░▒░ ░ ░▒ ▒  ░▒ ▒▒ ▓▒
░  ░      ░ ▓██ ░▒░ ░ ░ ▒  ░  ░ ▒ ▒░   ░  ▒   ░ ░▒ ▒░
░      ░    ▒ ▒ ░░    ░ ░   ░ ░ ░ ▒  ░        ░ ░░ ░
       ░    ░ ░         ░  ░    ░ ░  ░ ░      ░  ░
            ░ ░                      ░
MyLock - The working time logger by antic_eye ;)

Use this tool to track your productivity. MyLock generates an
encrypted database file that contains timestamps and "in"
or "out".

When you call myLock with "lock" it tracks:
01.01.2016T08:15 Hans.Meiser Out

When you call without args it tracks:
01.01.2016T08:19 Hans.Meiser In

When you want to now your times, call it with "-query". It will
read the db and calculate your working time beginning with the
first "In" per day, ending with the last "out".

To automate the tracking, use "-init" and myLock will generate
Windows Scheduled tasks for screen lock/unlock and session
login/-out.


Usage: mylock.exe [-q] [-r] [-i] [-d] [-h] [-j <Time|Direction">] [<lock>]

-q (--query)    Call the db and get your working times.
-r (--raw)      Get all logged events
-i (--init)     Create windows tasks (you need elevated permissions for this one!
-d (--deinit)   Remove windows tasks (you need elevated permissions for this one!
-h (--help)     Show this usage screen.
-j <Time|Direction"> (--inject) Use this for debugging only!