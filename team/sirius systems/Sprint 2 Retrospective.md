# Sprint 2 Retrospective
* This file contains notes written by each member that cover the things we discussed in our end of sprint review

## Hunter:
### What went well:
* Over the course of sprint 2 I made good headway with Identity, finishing up all currently required authentication features with the exception of resetting passwords which will carry into sprint 3 as a 2 point user story and shouldn't cause too much issue. Additionally, email verification using the SendGridApi was fully integrated and I felt that it went pretty well. Overall, by the end of sprint 2 we have a working foundation for user accounts and authentication including email verification with Identity and I'm very happy with that.
### What could have gone better:
* Implementing the SendGridApi was frustrating at times as I didn't quite understand exactly what I was looking to do as this was my first time working with authentication and email verification systems. In hindsight, I would have spent more upfront time towards the beginning of the sprint (or in the planning phase) making sure that I was more familiar with how these systems worked. While it didin't impede my ability to complete my user stories, it did create a bit of stress and the implmentation process wasn't as smooth as I would have liked. I also felt like I didn't get to write very many tests during the previous sprint and sprint 2, so while I'm unhappy with this aspect it is something that i'm looking forward to working on in sprint 3.
### Plans going into sprint 3:
Going into sprint 3 I plan on finishing the password reset feature which will bring my identity work to a close for the time being. I also plan to work on the foundation for the milestone and achievement features / page of StorkDork. This will include getting basic milestone information working, as well as basic badges displaying on a neat and consistently styled page. I will also focus on writing NUnit tests for this feature to ensure proper routing and data results match what should be expected.

## Christian:
###  What went well:

- sprint 2 consisted of me working my way through SD-37. Working on this user story really got me up to speed on dependency injection, fetching from JavaScript, and working with both Identity's users and our own StorkDork users.

###  What could have gone better:

- The prediction of effort points was off, SD-37 was a 2 point story, but took up all of my time during sprint 2. working with identity and finding out what I needed in order to find out what the current user was, really took a lot of time. In the future, I think the point value of anything that is new and not something a person is used to, should be higher. A higher point value will allow the person who works on the story to allocate a sensible amount of time to each story. With SD-37 taking all of my time, my 4 point user story is left for the next sprint, which doesn't feel good to me because I feel behind.
- I also tried my hand at some JEST to test all my JavaScript that I've been working on. unfortunately I wasn't able to deliver on JEST testing because the app doesn't like when/how I exported my functions, as it came up as an error and refused to run the JavaScript.

###  Plans going into sprint :3

- Going into sprint 3 I plan to finish SD-44 from sprint 2, and SD-45, which I made sure was something much easier than my 4 point story. Most of this work will revolve around the sightings popups on the map page, which is where most of my work has been, so I know the place pretty well. I also want to try and deliver on some unit testing (especially as we will be required to show our unit tests) so that I can ensure things are working how I want them to.

## Alice:

### What went well:

- I was really satisfied with how the implementation of my user stories turned out overall. I worked on adding features to the bird detail page to see a bird's related species, as well as a map view of the nearest recent sighting of the bird. It was my first time working with leaflet, but the groundwork from Christian's previous work helped me go into the sprint with a better understanding. My previous work gathering info & implementing the taxonomic search also helped me with my stories. I also worked with the geolocation API to get user location data for the first time, which I found to be really valuable experience.

### What could have gone better:

- I thought there was room for improvement on the logic behind identifying "related birds", as looser search terms can lead to weak/questionable relations being returned. I think I could've done more research beforehand to better hone what constitutes a related bird, and getting clearer definitions for my user stories in general. I had also attempted using jest for javascript testing this sprint, but wasn't able to successfully implement it.

### Plans going into sprint 3:

- Putting more time into doing preliminary research and refinement to make sure my user stories are well-defined and deliver better value to the application and end user. I also want to make sure I implement more robust testing this sprint by integrating jest for javascript testing.

## Uju:
###  What Went Well  

- Reflecting on the progress that I made in Sprint 2, I made sure the user features worked properly and cleaned up the UI.  

- I also believe that the bird log features were something that went well for me the most because it took the longest to do. During this part, I just wanted to test the functionality of the features, and they worked as they were supposed to, so I was really happy about that.  

### What Could Have Gone Better  

- There are a lot of things that could've gone better.  

- Several things could have gone better.  

- I need to invest more time in understanding how to interact with models and scripts. As someone not familiar with databases, I often found myself stuck on errors without knowing why.  

- Running scripts with incorrect table names caused unnecessary delays. I spent nights troubleshooting an issue that turned out to be a simple naming mistake, costing me valuable time.  

- I need to be more comfortable asking for help. I often hesitate, but seeking help would make progress smoother, and I plan to work on this moving forward.  

### Plans Going Into Sprint 3  

- For Sprint 3, my plans are to do what I didn’t do for Sprint 2 based on the user stories.  

- I will work on ensuring that logged-in users can log in and create sightings and checklists without making it a user dropdown. Once I do that, we can be smooth sailing with making more user-based content since bird logs and sightings are a main feature. Also, I really need to do testing.


