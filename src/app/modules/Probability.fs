module Probability

open System

type Probability = Probability of float

let calculate value (Probability propability) = value * propability

let create value = // todo: use Option
    if value < 0. || value > 100. then raise (ArgumentOutOfRangeException("value"))
    Probability (value / 100.)
