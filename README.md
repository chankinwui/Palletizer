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
The MPL problem and the Distributor Pallet Loading (DPL) problem (place different size boxes into a fixed size pallet) are much like chess playing, except that there is no King to be captured. The DPL problem is believed to be NP-hard, that is you cannot tell "win or lose", but you can say "good or bad" by experience (after trial and error for a few or more than billion time). The MPL problem is easier, there should be an optimial solution in given conditions, normally it is place as many boxes as possible in a stable manner. Human is clever and lazy, we can transform the DPL problem into MPL problem by using standard size boxes. And use many heuristic patterns to find the optimial solution.

But I am stupid but lazy too, so I bruteforce all the "possible" moves and let the computer do the hardwork. The following assumption should be clear:

- A new box will be placed to the corner of an existing boxes (align the corner to corner. In some cases may need to align edge to midlle,edge to 1/3 of width or even 1/10. But you will not place in a random position along the edge normally.)
- The pallet size and boxes' size should be "reasonable",for example the number of boxes should be less than 20 in a layer (<20 levels in the decision tree which should equal to the Master of Masters). If need more, you can add some boxes manaully (like the open book in Chess).

## Evaluation functions

### Touched perimeter
1. ![1666870815726](https://user-images.githubusercontent.com/3295412/198274984-75000732-200e-4439-bd6b-013321faaaf5.png)

