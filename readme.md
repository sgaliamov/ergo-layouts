# edLayouts

Is a utility to examine layouts for ErgoDox keyboard.

## Why?

There are many tools and methodologies out there already that evaluate keyboard layouts.
But they all have the one [fatal flaw](https://www.drdobbs.com/windows/a-brief-history-of-windows-programming-r/225701475): they are based on a standard keyboard.

Most standard keyboards are shifted: upper rows slightly moved left relatively lower rows.
What is completely reasonable for the right hand.
But the only explanations, that I can find for why do we have the same shift for the left hand, is reactionism, and is that manufacturers want to save money on production.

ErgoDox keyboard is very different.
Except for the extra key clusters for thumb fingers, it's slightly bigger than a typical laptop keyboard, and all buttons are placed strictly vertical.
On a regular keyboard for example, you don't have to tilt your wrist when you press the button N because of the shift.
On the other hand, in the literal and figurative sense, the button B is more reachable now on ErgoDox.

On a regular keyboard it's much easier to use pinky fingers on the top row.
On my laptop I can reach P and Q without any efforts, but on ErgoDox it's not really comfortable.
Maybe it's because of "bad habits" or adaptation period, but I just don't see how should I place my hands to fully enable pinkies, even if I want.

When you buy a keyboard that costs almost 400€, you probably ~~insane~~ want to use it 100%.
Initially I planned to configure peripheral buttons only.
But since with a non-standard keyboard I had to learn how to type again, I thought why not learn a new layout?

The only [Dvorak](https://en.wikipedia.org/wiki/Dvorak_keyboard_layout) came to mind.
But after a short investigation it turned out that Dvorak is not much better than Qwerty.
And some people aren't particularly keen on [Colemak](https://colemak.com/).

For that reason people have created quite a few different layouts.
In addition to well known Dvorak and Colemak we have:

- [Workman](https://workmanlayout.org/).
- [Carpalx](http://mkweb.bcgsc.ca/carpalx/).
- [Capewell](http://www.michaelcapewell.com/projects/keyboard/layout_capewell.htm).
- [Norman](https://normanlayout.info/).
- and many others. In my research I found and tested around 25 different designs.

As it usual happens, great variety slows down and [complicates](https://en.wikipedia.org/wiki/Overchoice) the selection process.
To help myself make a conscious choice, I start to thought about what is important to me.

## Estimation

What I find important:

1. Support for modern English only.

    For that purpose I used technical literature of approximately 10,000,000 characters in total to do comparisons.
    [Statistics](docs/statistics.md) that I collected slightly differs from that you can get using "Alice's Adventures in Wonderland" or "Moby Dick or The Whale".
    It's fine for me, because, to my shame, I do not plan to write such kind of texts.

2. Alternate hands less.

    Colemak is often criticized for the fact that T and H keys are placed on the different sides of a keyboard. It breaks the rhythm of the typing. This and the fact that the one of the most used vowels, A and O, are intended for pinkies, turned me away from using Colemak.

3. The keys of the most commonly used [digraphs](https://en.wikipedia.org/wiki/Digraph_(orthography)) should be placed close to each other.

    For example, E and R letters create the most used combination in English. Even Qw**er**ty has it, but many popular layouts (Workman, Norman, Colemak, etc.) ignore this for a reason that I can not explain.

4. Type less with one finger continuously.

    Just positioning keys of digraphs close to each other is not enough.
    It's slower to type, when keys are placed vertically, because a finger need to travel some distance to reach a next position.
    Much comfortable and faster to press next key with another finger.
    For that reason, it's a big loss for Workman and Dvorak to put O and E in the same row next to each other, this combination is used very rarely.

5. Minimize usage of pinkies.

    The little finger is already overused when pressing `Shift` and `Ctrl` buttons. For the weakest finger it's too big load.

6. The usage of hands should be balanced.

    It's hard to say which balance is the best.
    It sounds logical that the load on hands should be equal.
    But from other point of view, the left hand is used more often because we use a mouse with the right hand and place the most of shortcuts on the left side.
    To not overcomplicate, I decide stick to balanced option.

7. Punctuation marks do not have to be in the standard position.

    Since they are completely legitimate elements of a text we can benefit from placing them on better positions.
    Dot is used more often than a half of letters.
    Why  have it on an awkward button?

8. Shortcuts are not really important.

    Since we use ErgoDox keyboard we can configure extra layers and have almost any shortcut anywhere.
    The authors of many layouts deliberately left the Z, X, C, V buttons in place to preserve the familiar experience.
    It makes sense for regular keyboards, and I would care about it if I didn't have ErgoDox. I thought initially. The biggest problem with that, is when you have to switch to another language, all shotcuts are moved to the original places and you have to remember two sets of shotcuts and current language. It's really annoying.

Having all this requirements I started this project.
I wanted to try F# and scope of this assignment was perfect for it.
Why do I think that this estimation method is applicable?
Because the results that it gives, correlates with other estimations. Qwerty is bad, MTGAP is good as expected.

ErgoDox is such a keyboard for which you first need to learn a functional programming language with [static typing](https://en.wikipedia.org/wiki/Hindley%E2%80%93Milner_type_system), and then create a utility to find the most optimal configuration for it.
Joke.
A functional language with [dynamic typing](https://en.wikipedia.org/wiki/Lisp) is also suitable.

## Custom layout

In the process of creating the evaluative method, it became clear that none of the existing keyboards would fit it.
And even the first timid attempt to create a layout gave good results.
After some number of attempts I came to the following result:

``` text
' h i g ,    ; y o l q
w n a t .    f s e r p
z x c v b    k d u m j
```

Here is the full configuration for [ErgoDox](https://configure.ergodox-ez.com/ergodox-ez/layouts/EWljA/55ADn/0).
I can not say that I 100% happy wiht the result. There are some 

## Results

1. [All layouts results](docs/layouts.md).
2. [Comparison table](docs/results.xlsx).
3. [Configuring Excel file](docs/layouts.xlsx) that was used to do evaluations and the design.
4. [File](docs/patorjk.json) for [patorjk.com](http://patorjk.com/keyboard-layout-analyzer/#/main).
5. [Statistics](docs/statistics.md) of sampling texts.

## Summary

Do I recommend to buy the ErgoDox keyboard?
Er... not sure.
This keyboard is definitely not for everyone.
If you are not keyboard geek, or you are not ready for a radical new experience, then more traditional ergonomic keyboard is much wise option.

There is another trap that you can get into.
If you are perfectionist, like me, you risk to spend a lot of time trying to find a perfect configuration.
But the truth is: **there is no perfect configuration**.
Almost every attempt based on statistics and common sense will provide better result than Qwerty or Dvorak.
The time, that you spend on configuring it and learning how to type, most probably will not pay off.

Everything very depends on the estimation method, used keyboard, sampling texts, and personal preferences of an author.
It will be always a compromise.
Everything depends on what you value.

If you need just a good layout I recommend to look at:

1. [Capewell](http://www.michaelcapewell.com/projects/keyboard/layout_capewell.htm) and [Mtgap](http://mtgap.bilfo.com/completed_keyboard.html) are good balanced keyboards that are produced using [genetic algorithm](https://en.wikipedia.org/wiki/Genetic_algorithm).
2. [Asset](http://millikeys.sourceforge.net/asset/) has good balance and it should be easy to switch from Qwerty.
3. [Colemak](https://colemak.com/) not really bad if you switch D and H keys. It's much better alternative than Dvorak.
4. If you are not happy with Colemak you can look at [Norman](https://normanlayout.info/) or [Workman](https://workmanlayout.org/). Very worthy options.

## Useful links

- <http://patorjk.com/keyboard-layout-analyzer/#/main>.
- <https://elliotgeorge.net/2018/11/22/the-kaehi-keyboard-layout/>.
- <http://mkweb.bcgsc.ca/carpalx/>.
- <https://kennetchaz.github.io/symmetric-typing/results.html>.
- <https://geekhack.org/index.php?topic=67604.50s>.
- <https://mtgap.wordpress.com/2009/08/07/optimized-evolutionary-algorithm-for-keyboard-design-part-1/>.
