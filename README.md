# EZ makes tweening EZ

  EZ allows to script animations(or other continual code) as a queue of delegates. 
  You can animate objects, call functions and use branching and variables during animation.
  Lerp functions are useful inside Tween actions.
  
  All EZQueue instances are running in host object EZRunner.
  
  Usual EZQueues are unloaded on scene changes to prevent animating destroyed objects, 
  but you can spawn one with EZ.Spawn(true) to animate DontDestroyOnLoad objects.

  ``` c#

  private EZQueue ez;
  private Text label;
  private Image buttonNext;

  ...
  // Spawn tweener instance
  ez = EZ.Spawn(); 

  // For 1.5 seconds call function each frame with tweener progress(from 0 to 1) as argument
  ez.Tween(1.5f, pt => {

    // Use easing function in tweening
    var et = EZ.BackIn(pt);

    // Animate scale
    label.transform.localScale = Vector3.Lerp(Vector3.zero,Vector3.one,et);

    //Animate position
    label.transform.localPosition = Vector3.Lerp(Vector3.down*500,Vector3.zero,et);

    // Animate color(linear)
    label.color = Color.Lerp(Color.clear,Color.white,pt);

    // Animate label value 
    label.text = Mathf.Lerp(0,totalScore,CubicOut(pt)).ToString("0");
  }).Delay(1);//Add 1 second delay

  // Start looping 
  ez.Loop().Tween(pt=>{
    buttonNext.transform.scale = Vector3.one + Vector3.one*(0.1f*Mathf.Sin(6.293f*t));
  });

  ...

  OnClickNext(){
    ez.Kill();

    // Spawn tweener instance. bool arg - Allow tweener to play on scene change 
    EZ.Spawn(true).Tween(t=>{
      // Hide result UI
    });
  }
  ```
