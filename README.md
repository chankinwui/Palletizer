# Palletizer

## Introduction
This is a simple yet effective, and interesting program  to solve pallatizing problem. Specifically, the Manufactuere Pallet Loading (MPL) problem (place same size boxes into a fixed size pallet), which is beleived to be a NP-complete problem. There are many papers and mathematical models to describe and solve it. In this program, I use the chess method (traditional approach like Deep Blue, NOT the one in AlphaGo Zero).

## Chess
The idea of chess programming is very simple, but of course there are many genius skills. Here is a simplified logic:
![image](https://user-images.githubusercontent.com/3295412/198256361-151a2ed6-6f7d-409b-89e6-bd5079f76bcd.png)

Either you are a Master or Novice depends on how "fast" and how "deep" you can look ahead, and the strategies chosen. A Master can foresee more than 10 rounds (we call it level here) in less than a few minutes. Of course he/she does not move by random or bruteforce all the possible moves, and here comes the concept of evaluation functions and decision tree. 
![image](https://user-images.githubusercontent.com/3295412/198264140-840cacc3-7457-4f58-a603-d65717ee9057.png)

The goal is capture the King as soon as possible, which is find the shortest path to get the maximum score.

## Pallet
The MPL problem and the Distributor Pallet Loading (DPL) problem (place different size boxes into a fixed size pallet) are much like chess playing, except that there is no King to be captured. The DPL problem is believed to be NP-hard, that is you cannot tell "win or lose", but you can say "good or bad" by experience (after trial and error for a few or more than billion time). The MPL problem is easier, there should be an optimial solution in given conditions (non-guillotine pattern), normally it is place as many boxes as possible in a stable manner. Human is clever and lazy, we can transform the DPL problem into MPL problem by using standard size boxes. And use many heuristic patterns to find the optimial solution.

But I am stupid but lazy too, so I bruteforce all the "possible" moves and let the computer do the hardwork. The following assumption should be clear:

- A new box will be placed to the corner of an existing boxes (align the corner to corner. In some cases may need to align edge to midlle,edge to 1/3 of width or even 1/10. But you will not place in a random position along the edge normally.)
- The pallet size and boxes' size should be "reasonable",for example the number of boxes should be less than 20 in a layer (<20 levels in the decision tree which should equal to the Master of Masters). If need more, you can add some boxes manaully (like the open book in Chess).

## Evaluation functions

### Touched perimeter (Assume width=2,height=1)
1. touched=2+2, total=6+6, score=(2+2)/(6+6)

![1666870815726](https://user-images.githubusercontent.com/3295412/198274984-75000732-200e-4439-bd6b-013321faaaf5.png)

2. touched=1+1, total=6+6, score=(1+1)/(6+6)


![1666871243520](https://user-images.githubusercontent.com/3295412/198276149-90fdef76-02fa-4c72-be3d-d9becc029d54.png)

### Connected boxes
1. score=connected=1+2+1


![1666871382357](https://user-images.githubusercontent.com/3295412/198276593-345c318e-37af-4c03-9e7b-2b7d78d66321.png)

2. score=connected=2+2+2


![image](https://user-images.githubusercontent.com/3295412/198276700-2051f183-3901-497e-8685-5760f3ea33c1.png)

### Bounding area
1. score=8/9. This is interlock pattern which is preferred in many cases. Multiply this if necessary. Reduce the total score if the hole's size is more than the minimum side of the box,since it will make the boxes unstable.

![image](https://user-images.githubusercontent.com/3295412/198278257-d70f8395-ca13-4df8-9a16-4ffd89885d4c.png)

2. score=8/8

![1666871942282](https://user-images.githubusercontent.com/3295412/198278472-6029ecb5-9e1f-4c37-a562-da3195c7bc90.png)

### Final shape
1. Preferred since we will wrap the pallet in real world


![1666872431065](https://user-images.githubusercontent.com/3295412/198280124-3ea42e19-7917-46f1-b8dd-addd1450bd1f.png)


2. Not preferred

![image](https://user-images.githubusercontent.com/3295412/198279809-65a7a725-9248-4c48-84d4-f9205155d4f3.png)


## How to use and examples

### Description file
Create a text file and organize the parameters in json format. The unit is arbitrary.

maxWidth,maxHeight=pallet size, no box should outside this

boxLongEdge,boxShortEdge=box size

targetLevel=sometime we may need to find the best pattern with a few boxes only. Set to 9999 if not use.



![1666872974744](https://user-images.githubusercontent.com/3295412/198281836-d599d898-53ae-45d1-8b96-54cf7301ec89.png)


### Interface

Press "Create new" and select a description file, press "GetScore" to evaluate the current pattern described by the file.

Press "GetAll" to find the possible combinations. Press "Stop" at any time.

After the result is ready (maybe a few seconds or a few days!) ,press "Display result".

![image](https://user-images.githubusercontent.com/3295412/198281658-c53abb87-d2ed-4e18-831a-1976bd1a5735.png)


### Examples
4 boxes interlock pattern, add 4 more boxes to find the best pattern

{"maxWidth":200,"maxHeight":200,
"boxLongEdge":40,"boxShortEdge":20,
"targetLevel":8,"boxes":[
{"box":[60,60,40,20]},
{"box":[100,60,20,40]},
{"box":[60,80,20,40]},
{"box":[80,100,40,20]},
]}

![1666874689653](https://user-images.githubusercontent.com/3295412/198287706-fb3e74f3-bff9-4b65-a447-1bff9647401f.png)

![image](https://user-images.githubusercontent.com/3295412/198288709-c168bacc-ae9f-4a9b-a167-d27964427d1c.png)


The score is negative since the hole is bigger than the shorter side of the box.


#### A real example (used in 250mL drinks)
{"maxWidth":120,"maxHeight":100,"boxLongEdge":36,"boxShortEdge":20,"targetLevel":99999,"boxes":[
{"box":[0,0,36,20]},
{"box":[0,20,36,20]},
{"box":[0,40,36,20]},
{"box":[0,60,20,36]},
{"box":[20,60,20,36]},
{"box":[36,0,36,20]},
{"box":[72,0,36,20]},
]}

![1666875068926](https://user-images.githubusercontent.com/3295412/198289141-efcb29f1-0f7a-4285-b53d-622be0448c1a.png)

![image](https://user-images.githubusercontent.com/3295412/198289373-021ca180-e6b5-4abc-b293-78d4e9e4bf5a.png)

