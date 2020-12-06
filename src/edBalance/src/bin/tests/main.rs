use itertools::Itertools;

fn main() {
    // let a = vec![(1..=5), (6..=7)];

    // let b: Vec<_> = a
    //     .into_iter()
    //     .group_by(|x| true)
    //     .into_iter()
    //     .flat_map(|(_, x)| x.into_iter())
    //     .collect();

    // println!("{:?}", b);

    let a = vec![1];

    let b: Vec<_> = a.into_iter().tuple_windows().map(|(a, b)| a + b).collect();

    println!("{:?}", b);
}
