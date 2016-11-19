# Life Coach

Life Coach is a solution that aims to make life easier to manage your tasks from the command line.

The life coach uses Google Calendar to manage your tasks.

In order for the console application to work you will need a client_secret.json file in the output directory (obtained from here https://console.developers.google.com/ for the Calendar API). You should replace the client_secret.json files
in the solution with the one you obtain._

Compiling the solution will result in a LifeCoach.exe

Currently you can run the following commands:

	LifeCoach note-task "My task"

This will create a task in a google calendar called 'Life Coach' under the users account calendar. If the user will be asked authorisation via a web browser.

You can then ask what tasks you currently have:

	LifeCoach list-tasks

Which will print out the following:
    
          Id                    | Description       | DueDateTime
     -------------------------- | ----------------- | -----------:     
     tq0gv5nkg01c89p8b0atrt5hrk | My task           |

You can set a due date & time on a task using the -d switch:

	LifeCoach note-task "Meet santa clause" 2016-12-25 00:00:10

And then view all the tasks by a given date by specifying the -d switch:

    LifeCoach list-tasks -d 2016-12-25

