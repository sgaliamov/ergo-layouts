use ed_balance::models::Cli;
use ed_balance::run;
use std::iter;
use std::process;
use structopt::StructOpt;

trait Tr {
    type Inner;

    fn next(val: &Self::Inner);
}
trait Base {
    fn next(&self) -> i32;
    fn foo(&self) -> i32 {
        *&self.next()
    }
}

trait Child: Base {
    fn next(&self) -> i32 {
        1
    }
}

trait Tr2<T = i32> {
    fn next(&mut self) -> T;
}

struct S1 {}

impl S1 {
    fn new() -> S1 {
        S1 {}
    }
}

// impl Tr for S1 {
//     type Inner = i32;

//     fn next(val: &Self::Inner) {}
// }

impl Tr2 for S1 {
    fn next(&mut self) -> i32 {
        1
    }
}

impl Tr2<String> for S1 {
    fn next(&mut self) -> String {
        "val.clone()".to_string()
    }
}

// impl Tr for S1 {
//     type Inner = String;

//     fn next(val: &Self::Inner) {}
// }

fn foo(a: &i32, b: &i32) -> i32 {
    a + b
}

fn main() {
    let v = vec![1, 2, 3, 10];

    let woo = foo.clone();

    let r: i32 = v.iter().map(|x| move |y| woo(y, x)).map(|f| f(&1)).sum();

    // let args = Cli::from_args();
    // if let Err(e) = run(&args) {
    //     eprintln!("Calculations failed: {:#?}", e);
    //     process::exit(1);
    // }t

    print!("{}", r);

    // test(args);

    // test(args);
}

fn test(cli: Cli) -> String {
    cli.frozen
}
