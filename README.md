#KerbalStats
KerbalStats is a KSP mod for keeping track of extra information about the
kerbals in your game. It has been designed to be easily extended by other
mods wishing to store kerbal specific information. Also, it is easy for
mods to query KerbalStats for the stored information.

KerbalStats comes with two built-in modules that are both useful and act as
sample implementations: *gender* and *experience*.

##*gender* module
The gender module is very simple and stores only a single value
representing the gender of the kerbal. It currently checks the kerbal's
name for any forms that seemed to indicate a gender to the author and
assigns a gender as appropriate, or should there be no obvious forms, the
gender is chosen randomly.

For mods wishing to query the gender of a kerbal, the query string is
simply **"gender"** (the module name). Any parameters will be silently
ignored. The returned value is either "M" or "F" for male and female.

##*experience* module
The experience module is rather complicated. It keeps track of the amount
of game-time assigned kerbals spend doing various tasks. Along with the
task, the location (planet) and situation (landed, flying, etc) of the
kerbal while "performing" that task. It behaves a lot like a time-punch
system. The experience module puts ***no*** meaning on the logged
experience: that is up to any mods querying the kerbal's experience.

The logged task is determined by the part to which the kerbal has been
assigned, or "EVA" for kerbals on EVA. For kerbals on board a vessel, the
task is determined by checking a database for the part and seat in that
part for the task assigned to that seat. Should the lookup fail, the task
is set to "Passenger". The database is defined in **seat_tasks.cfg**

The tasks defined in KerbalStats are "Science", "Pilot", "Command",
"Passenger" and "EVA". Other mods are free to define their own tasks and
assign them to seats in parts.

### *seat_tasks.cfg* database
This is simply a standard KSP config file. Mods wishing to define their own
tasks and seat assignments need only to supply their own config file.

The config file has a root node named **KSExpSeatMap** with subnodes named
**SeatTasks**. *SeatTasks* nodes contain only value assignments:

* **name=part-name** This is the part name for this *SeatTasks* node (eg
  *Large_Crewed_Lab* for the stock science lab).
* **default=task** is the task to assign should the kerbal's seat not be
  listed in the node (eg, the part has no IVA, or most seats have one task
  while a few exceptions have another task).
* **seat-name=task** This is the name of the seat transform as specified in
  the part's internal config.

For mods wishing to query the experience of a kerbal, the query string is
of the form **"experience:task=Pilot,body=Kerbin,situation=ORBITING"**.
However, the parameters are optional: **"experience"** will return the
total experience for all tasks, bodies and situtations. Also, ommitting any
of the *task*, *body* or *situation* parameters will return the total for
the ommitted qualities. eg, **"experience:body=Eve"** will return the total
experience for all tasks and situtations in Eve's sphere of influence. The
returned value is the string representation ("G17" format) of the amount of
time, in seconds, the kerbal has logged for the relevant task, body and/or
situation.

It is up to the mod to give actual meaning to the experience.

##Querying KerbalStats

Querying KerbalStats from a mod is very easy: include KerbalStatsWrapper.cs
in your project, change **ModName** in the namespace to the name of your
mode, and then call KerbalExt.Get() with the ProtoCrewMember object
representing the kerbal and the query string.

The general format of the query string is
**"&laquo;module-name&raquo;:&laquo;module-params&raquo;"**.
*module-params* is defined by the module.

The query always returns the value as a string, or **null** if something
went wrong. If **null** is returned, then something will have been printed
to the KSP logs.

KerbalStatsWrapper.cs has been writted such that there is no need to link
against KerbalStats.dll. If the dll is not present, then KerbalExt.Get()
will return **null** and log the issue.

**NOTE**: KerbalStatsWrapper.cs is licensed using the GNU LGPL (as is the
rest of KerbalStats). This means that mods are free to use
KerbalStatsWrapper.cs without worrying about their own license so long as
KerbalStatsWrapper.cs itself remains under the GNU LGPL.

##Extending KerbalStats

At this stage, extending KerbalStats requires linking agaist
KerbalStats.dll. To create a module, derive from IKerbalExt. It is probably
best to study the gender (Gender/Gender.cs) and experience
(Experience/Tracker.cs) modules to see what is needed. The gender module
uses only a ConfigNode value while the experience module uses a ConfigNode
node. It is then best to put the extension module in a directory that is
guaranteed to come after the KerbalStats.dll (KSP dll load order issues).
