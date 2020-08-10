Introductions

1.	The plugin is used for automatically creating the flying route for the object on the basis of several points that have been already known. It will simulate the track of flying object realistically.
2.	Please put footprint RouteGenerator.cs and foorprint RouteEditor.cs  in the folders of ../ARG/RouteGenerator on the object you want to operate.
3.	Edit related parameters in the footprint RouteEditor.cs.



Parameters

Several important parameters in the footprint RouteEditor.cs.
startTime: the time when the object start to move. 
endTime: the time when the object finish the action.
stepLength: the step length of the moving object. And the footprint will automatically complete 1/stepLength plots between two states.
stateList: save the 


   
   
   Methods of editing

     There are four buttons alongside every state of stateList.
     Button M: move the object to the current position.
     Button S: save the current status of the object.
     Button +: add a new status at current position.
     Button -: delete the current position.



 Specific declaration of stateList
1.	You can not delete the last status in the stateList.
2.	You should better keep at least 6 status if you want the footprint to run normally.( 
The first and the last three status are used for calculate the speed and the acceleration of the object. )
 

We sincerely hope that our program will help you with your game development.