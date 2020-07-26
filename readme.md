# edLayouts

Is a utility to examine layouts for ErgoDox keyboard.

When you buy a keyboard that costs almost 400€ you probably want use it 100%.
Initially I planned to configure peripheral keys. Но так как пришлось все равно учиться печатать заного, я подумал почему бы не выучить новую раскладку?
после недолгого расследования выяснилось что дворак не намного лучше кверти. и от колемака люди не особо в восторге.

в процессе создания оценочного метода стало ясно что ни одна из существующих клавиатур не подойдет под него.
и даже первая неглубокая попытка создать раскладку которая бы подходила бы под метод дала хорошие результаты.

я всегда пердпочиталь шоткаты.
что хотелось
- меньше черодовать руки. это важно. например колемак часто критикуют за это.
- меньше повторять пальцы.
- расположить часто используемые диграфы на одной стороне.
- шоткаты во внимание не принимались так так на клаве можно настроить экста слои.

## Why?

ErgoDox is such a keyboard for which you first need to learn a functional programming language with static typing, and then create a utility to find the most optimal configuration for it.
Joke.
A functional language with dynamic typing is also suitable.

There are plenty tools already that estimate layouts.
But they all have the one fatal flaw: they based on standard keyboards.

ErgoDox keyboard is very different, and it's bigger than a regular laptop keyboard.
On a regular keyboard for example, you don't have to tilt your wrist when you press button N because all buttons are shifted.
On ErgoDox all buttons are strictly vertical.
On the other hand, in the literal and figurative sense, button B is more reachable now.

It's much easier to use pinky fingers on top row on a regular keyboard.
On my laptop I can reach P and Q without any efforts using pinkies, but on ErgoDox it's not really comfortable.
Maybe it's because of "bad habits" or adaptation period, but I just don't see how should I place my hands to enable pinkies 100%, even if I want.


Another reason, I wanted to try F# for some small project and scope of this assignment is perfect for F#.
I feel I returned 10 years back - learning how to type, learning how to program again. Very interesting experience.

## Results

1. [All layouts results](docs/layouts.md).
2. [Comparison table](docs/results.xlsx).
3. [Configuration for ErgoDox](https://configure.ergodox-ez.com/ergodox-ez/layouts/EWljA/latest/0).
4. [Configuring Excel file](docs/layouts.xlsx) was used to do evaluations.
5. [File](docs/patorjk.json) for [patorjk.com](http://patorjk.com/keyboard-layout-analyzer/#/main).
6. [Statistics](docs/statictics.md) of sampling texts.

## Summary

Do I recommend to buy the ErgoDox keyboard? Er... not sure. This keyboard is definitely not for everyone.
If you are not a keyboard geek, or you are not ready for a radical new experience, then more traditional ergonomic keyboard is much wiser option.

There is another trap that you can get into. If you are perfectionist, like me, you risk to spend a lot of time trying to find the best possible configuration.
But the truth is that there is no perfect configuration. Almost every attempt based on statistics and common sense will provide better result than Qwerty or Dvorak.
The time that you spend on configuring it most probably will not pay off.

Everything depends on the estimation method, used keyboard, and personal preferences of an author.

It will be always a compromise. Everything depends on what you value.

In addition, authors of those layouts assume that you use fingers strictly vertically.
But in my case it does not work.
For example, it's much more comfortable for me to use ring finger for P and Q instead of pinky.
I don't know who is mutant in this case.

## Scoring

1. Efforts based on configuration. Should go down.
1. Same fingers. Should go down.
1. Characters typed with one hand without changing hand should scored more.
1. Hands balance. Left and right hands should be used equally.
1. Inward rolls are more preferable than outward.
1. Fingers statistic.

Why do I think that this estimation method is applicable? Because the results that it gives, correlates with other methods. `Qwerty` is bad, `MTGAP` is good as expected.

## To run

``` sh
edLayout.exe -i ./samples -l ./layouts/qwerty.json -o ./results.csv
```

## Useful links

* <http://patorjk.com/keyboard-layout-analyzer/#/main>.
* <https://elliotgeorge.net/2018/11/22/the-kaehi-keyboard-layout/>.
* <http://mkweb.bcgsc.ca/carpalx/>.
* <https://kennetchaz.github.io/symmetric-typing/results.html>.
* <https://geekhack.org/index.php?topic=67604.50s>.
* <https://mtgap.wordpress.com/2009/08/07/optimized-evolutionary-algorithm-for-keyboard-design-part-1/>.
