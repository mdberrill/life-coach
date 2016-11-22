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
<pre>    
     Id     | Description       | Due Date    | Done 
     ------ | ----------------- | ----------- | ----    
     tq0gv5 | My task           | -           | No
</pre>

You can set a due date & time on a task using the -d switch:

	LifeCoach note-task "Meet santa clause" 2016-12-25 00:00:10

And then view all the tasks by a given date by specifying the -d switch:

    LifeCoach list-tasks -d 2016-12-25

To view all your tasks today:

    LifeCoach list-tasks

To view all your unplanned tasks, .i.e. those without a due date:

    LifeCoach list-tasks -u

To view all your tasks in the next X days, e.g. 7 days:

    LifeCoach list-tasks 7

To mark a task as completed use complete-task with the start of the Id:

    LifeCoach complete-task tq0

To undo this change and go back to incomplete you can use the -u switch

    LifeCoach complete-task tq0 -u