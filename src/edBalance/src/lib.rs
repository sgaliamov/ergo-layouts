#[cfg(test)]
pub mod tests {
    #[test]
    pub fn my_test_lib() {
        assert_eq!(2 + 2, 4);
    }
}

pub mod custom {
    use serde_json::{Map, Value};
    use std::collections::HashMap;

    pub fn create_digraphs_map(json: &Map<String, Value>) -> HashMap<char, HashMap<char, f64>> {
        json.iter()
            .map(|(digraph, value)| {
                let first = digraph.chars().nth(0).unwrap();
                let second = digraph.chars().nth(1).unwrap();
                (first, second, value.as_f64().unwrap())
            })
            .fold(HashMap::new(), |mut result, (first, second, value)| {
                result
                    .entry(first)
                    .or_insert(HashMap::new())
                    .insert(second, value);
                result
                    .entry(second)
                    .or_insert(HashMap::new())
                    .insert(first, value);
                result
            })
    }
}
