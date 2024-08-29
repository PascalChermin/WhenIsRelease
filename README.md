## Summary
I've decided to open-source a project I worked on several years ago which was hosted on a now expired website.  
It was a website which listed release dates of videogames by platform and region, allowed visitors to generate an iCal link to import in their own calendars to see upcoming releases, and tweeted (back when it was still Twitter) all releases on launchday with their cover images.

<sub>Snapshot from Wayback Machine</sub>  
![Snapshot from Wayback Machine](https://github.com/user-attachments/assets/e5ce3c81-e93b-415b-be38-ef342818b9a6)
<sub>The last tweet to go out</sub>  
![The last tweet to go out](https://github.com/user-attachments/assets/b59c727d-fbc4-4cb8-b354-a931c01c9c76)

## Stack
The backend was a .NET application which polled a GiantBomb API periodically for new and updated data. It would also tweet out all games which would be released daily, split into a maximum of 4 at a time, due to the image limitations of Twitter.  
  
The frontend was very basic Angular, you either search a game, or you filtered out your region and preferred platforms to get an iCal link. At first I wanted to include all titles which ever came out (so you could search in your own calendar app when a game was released), but this resulted in files that were too big for most calendar apps to process and sync. So I reduced it to only include upcoming releases which worked just fine.  

I added a Docker compose file as I used it to quickly fire up a dev database but wasn't into Docker that much at the time to containerize the whole app.
