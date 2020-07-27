# edLayouts

Is a utility to examine layouts for ErgoDox keyboard.

## Why?

ErgoDox is such a keyboard for which you first need to learn a functional programming language with static typing, and then create a utility to find the most optimal configuration for it.
Joke.
A functional language with dynamic typing is also suitable.

When you buy a keyboard that costs almost 400€ with taxes, you probably want to use it 100%.
Initially I planned to configure peripheral keys only. 
Но так как с не стандартной клавиатурой приходилось все равно учиться печатать заново, я подумал почему бы не выучить новую раскладку?

на ум приходил только дворак.
но после недолгого расследования выяснилось, что дворак не намного лучше кверти.
да и от колемака люди не особо в восторге.

по этой причине люди создали довольно много различных раскладок. и как обычно, большое разнообразие только замедляет и усложняет процесс выбора.

что бы выбор был осознанным, я задумался над тем что важно для меня.

## Estimation

There are plenty tools already that estimate layouts.
But they all have the one fatal flaw: they based on standard keyboards. And most standard are shifted: upper row slightly moved left relatively lower row.

ErgoDox keyboard is very different, it's bigger than a regular laptop keyboard, and all buttons are strictly vertical.
On a regular keyboard for example, you don't have to tilt your wrist when you press button N because all buttons are shifted.
On the other hand, in the literal and figurative sense, button B is more reachable now.

It's much easier to use pinky fingers on top row on a regular keyboard.
On my laptop I can reach P and Q without any efforts using pinkies, but on ErgoDox it's not really comfortable.
Maybe it's because of "bad habits" or adaptation period, but I just don't see how should I place my hands to enable pinkies 100%, even if I want.

что хотелось
- меньше черодовать руки. это важно. например колемак часто критикуют за это.
- расположить часто используемые диграфы на одной стороне.
- меньше повторять пальцы.
- минимизировать использование мизинца. мизинец и так чрезмерно используется при нажатии шифтов и контролов. для самого слабого пальца, это слишком большая нагрузка.
- распольжить знаки пепинания в более разумных местах, так как они вполне полноправные элементы текста.
- шоткаты во внимание не принимались так так на клаве можно настроить экста слои. и заучивание новых шоткатов никогда не было пробелемой для меня.

I wanted to try F# for some small project and scope of this assignment is perfect for F#.
I feel I returned 10 years back - learning how to type, learning how to program again. Very interesting experience.

## Scoring

1. Efforts based on configuration. Should go down.
1. Same fingers. Should go down.
1. Characters typed with one hand without changing hand should scored more.
1. Hands balance. Left and right hands should be used equally.
1. Inward rolls are more preferable than outward.
1. Fingers statistic.

Why do I think that this estimation method is applicable? Because the results that it gives, correlates with other methods. `Qwerty` is bad, `MTGAP` is good as expected.

для оценки я использовал не классическую литературу, как это часто делают, а книги по разработке по. около 10 млн символов текста.

## Custom layout
  
в процессе создания оценочного метода стало ясно что ни одна из существующих клавиатур не подойдет под него.
и даже первая робка попытка создать раскладку, которая бы подходила бы под метод дала хорошие результаты.

после некоторого числа попыток я пришел к следующему результату.

``` pre
z h i d k   ' f o l j
g n a t .   p s e r y
q v w c ,   ; u b m x
```

## Results

1. [All layouts results](docs/layouts.md).
2. [Comparison table](docs/results.xlsx).
3. [Configuration for ErgoDox](https://configure.ergodox-ez.com/ergodox-ez/layouts/EWljA/latest/0).
4. [Configuring Excel file](docs/layouts.xlsx) that was used to do evaluations and the design.
5. [File](docs/patorjk.json) for [patorjk.com](http://patorjk.com/keyboard-layout-analyzer/#/main).
6. [Statistics](docs/statictics.md) of sampling texts.

## Summary

Do I recommend to buy the ErgoDox keyboard? Er... not sure. This keyboard is definitely not for everyone.
If you are not a keyboard geek, or you are not ready for a radical new experience, then more traditional ergonomic keyboard is much wise option.

There is another trap that you can get into. If you are perfectionist, like me, you risk to spend a lot of time trying to find a perfect configuration.
But the truth is: there is no perfect configuration. Almost every attempt based on statistics and common sense will provide better result than Qwerty or Dvorak.
The time, that you spend on configuring it and learning how to type, most probably will not pay off.

Everything very depends on the estimation method, used keyboard, and personal preferences of an author.

It will be always a compromise. Everything depends on what you value.

Asset хороший баланс и должно быть легко выучить http://millikeys.sourceforge.net/asset/
Capewell and Mtgap V2 good balanced keyboard produced using evolution algorithms
Collemak not really bad if you switch D and H keys. Personally i cant use it because edge not effective positions of A and O letters. But still it's much better alternative than Dvorak.
Norman and Workman

## To run

``` cmd
edLayout.exe -i ./samples -l ./layouts/qwerty.json -o ./results.csv
```

## Useful links

- <http://patorjk.com/keyboard-layout-analyzer/#/main>.
- <https://elliotgeorge.net/2018/11/22/the-kaehi-keyboard-layout/>.
- <http://mkweb.bcgsc.ca/carpalx/>.
- <https://kennetchaz.github.io/symmetric-typing/results.html>.
- <https://geekhack.org/index.php?topic=67604.50s>.
- <https://mtgap.wordpress.com/2009/08/07/optimized-evolutionary-algorithm-for-keyboard-design-part-1/>.
