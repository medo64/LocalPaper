LocalPaper
============

Bitmap composer for Trmnl device.

The easiest way to play with it is to use [docker image](https://hub.docker.com/r/medo64/localpaper).

You can then run it locally using something like this:
~~~sh
docker run --rm -it -v ./example:/config --network=host medo64/localpaper:latest
~~~

Then you can configure your device to use custom server at port 8084 (`http://<ip>:8084`).

The following variables can be configured:

| Variable        | Default               | Description                                     |
|-----------------|-----------------------|-------------------------------------------------|
| `LP_HOST`       | (computername)        | Host name or IP at which web server will listen |
| `LP_CONFIG_DIR` | `/config`             | Configuration directory root                    |
| `LP_TIMEZONE`   | `America/Los_Angeles` | Time zone                                       |
| `LP_LOGLEVEL`   | `INFORMATION`         | Log level                                       |

To configure each display, you need to create a subdirectory containing devices MAC address with colons (`:`) removed.
If you are using just a single screen, you can use `any` directory or just place configuration files in `/config` directly.

Configuration file must be named `config.ini` and it looks something like this:

~~~ini
[Display]
Width=800
Height=480
Interval=300
TimeZone=America/Los_Angeles

[Time.0]
Top=0
Bottom=47
Left=0
Right=799
Format=dddd
Align=Left

[Time.1]
Top=0
Bottom=47
Left=0
Right=799
Align=Center
Format=yyyy-MM-dd

[Time.2]
Top=0
Bottom=47
Left=0
Right=799
Align=Right
Format=HH:mm

[Events]
Directory=Events
Top=49
Bottom=479
Left=0
Right=265

[Events.+1]
Directory=Events
Offset=24
Top=49
Bottom=479
Left=267
Right=532

[Events.+2]
Directory=Events
Offset=48
Top=49
Bottom=479
Left=534
Right=799
~~~

Each part has rectangle in which data is displayed (`Left`, `Right`, `Top`, `Bottom`) and `Offset` parameter controlling the time that will be displayed.
In addition, some entries (composers) might need a further configuration directory. By default that directory is the same as name of the section but it can be configured using `Directory` argument.
If you need to use the same composer multiple times, use a `.` followed by any characters to give it an unique name.

The following composers are currently available:

| Composer  | Extra                | Description                                        |
|-----------|----------------------|----------------------------------------------------|
| Events    | (subdirectory)       | Shows list of events based on subdirectory entries |
| Line      | `Thickness`          | Draws a line                                       |
| Rectangle | `Filled` `Thickness` | Draws a rectangle                                  |
| Time      | `Align` `Format`     | Shows time                                         |

For events, configuration format is as follows:

~~~ini
[2025-07-04]
Holiday=Independence Day

[2025-09-01]
Holiday=Labor Day

[2025-10-13]
Holiday=Columbus Day
~~~

Each section is a date with elements in it defining header (key) and content (value).
If multiple entries are found with the same key, they will be grouped together.
