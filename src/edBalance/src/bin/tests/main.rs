use std::thread;
use std::time::Duration;
use indicatif::{MultiProgress, ProgressBar};

fn main() {
    let m = MultiProgress::new();
    // let sty = ProgressStyle::default_bar()
    //     .template("[{elapsed_precise}] {bar:40.cyan/blue} {pos:>7}/{len:7} {msg}");

    let pb = m.add(ProgressBar::new(128));
    // pb.set_style(sty.clone());

    let pb2 = m.add(ProgressBar::new_spinner());
    // let spinner_style = ProgressStyle::default_spinner()
    //     .tick_chars("⠁⠂⠄⡀⢀⠠⠐⠈ ")
    //     .template("{spinner} {wide_msg}");
    // pb2.set_style(spinner_style);

    let t = thread::spawn(move || {
        for i in 0..128 {
            pb2.set_message(&format!("item #{}", i + 1));
            pb.set_message(&format!("item #{}", i + 1));
            pb.inc(1);

            thread::sleep(Duration::from_millis(8));
        }

        pb2.finish();
        pb.finish();
    });

    m.join().unwrap();
    t.join().unwrap();
}
