

class Plate {
    seed;
    numbers;
    rows_completed = 0;
    check_sum_1_30 = [0, 0, 0];
    check_sum_31_60 = [0, 0, 0];
    check_sum_61_90 = [0, 0, 0];

    constructor(seed) {
        this.seed = seed;
        Math.seedrandom(seed + "");
        var dict = new Array(3);
        for (let j = 0; j < 3; j++) {
            dict[j] = [];
        }
        var cols = [];
        for (var i = 0; i < 9; i++) {
            var col = generate_col(i);
            cols.push(col);
        }

        var rows_choose = generate_rows_check();

        for (var j = 0; j < 3; j++) {
            for (var i = 0; i < rows_choose[j].length; i++) {
                var k = rows_choose[j][i];
                dict[j].push(cols[k - 1][j]);
            }
        }

        this.numbers = dict;
        this.generateCheckSums();
    }

    generateCheckSums() {
        for (let j = 0; j < this.numbers.length; j++) {
            let check_sum = generateCheckSums(this.numbers[j]);
            this.check_sum_1_30[j] = check_sum.check_sum_1_30;
            this.check_sum_31_60[j] = check_sum.check_sum_31_60;
            this.check_sum_61_90[j] = check_sum.check_sum_61_90;
        }
        
    }

    static generatePlates(amount, list) {
        for (let x = 0; x < amount; x++) {
            list.push(new Plate("René" + x));
        }
    }
}

class Board {
    static check_sum_1_30 = 0;
    static check_sum_31_60 = 0;
    static check_sum_61_90 = 0;

    static row_1_completed = 0;
    static row_2_completed = 0;
    static row_3_completed = 0;

    static row_1_completed_seed = "";
    static row_2_completed_seed = "";
    static row_3_completed_seed = "";

    static plates = [];

    static pickedNumbers = new Set();

    static pickNumber(number) {
        Board.pickedNumbers.add(number);
        let check_sums = generateCheckSums(Array.from(Board.pickedNumbers));
        Board.check_sum_1_30 = check_sums.check_sum_1_30;
        Board.check_sum_31_60 = check_sums.check_sum_31_60;
        Board.check_sum_61_90 = check_sums.check_sum_61_90;

        if (Board.pickedNumbers.size >= 5) {
            Board.checkPlates();
        }
    }

    static checkPlates() {

        this.row_1_completed = 0;
        this.row_2_completed = 0;
        this.row_3_completed = 0;

        for (let i = 0; i < Board.plates.length; i++) {
            let plate = Board.plates[i];
            plate.rows_completed = 0;
            for (let j = 0; j < plate.numbers.length; j++) {
                if (
                    (plate.check_sum_1_30[j] & Board.check_sum_1_30) === plate.check_sum_1_30[j] &&
                    (plate.check_sum_31_60[j] & Board.check_sum_31_60) === plate.check_sum_31_60[j] &&
                    (plate.check_sum_61_90[j] & Board.check_sum_61_90) === plate.check_sum_61_90[j]
                ) {
                    plate.rows_completed++;
                }
            }
            if (plate.rows_completed >= 1 && !this.row_1_completed_seed) this.row_1_completed_seed = plate.seed;
            if (plate.rows_completed >= 2 && !this.row_2_completed_seed) this.row_2_completed_seed = plate.seed;
            if (plate.rows_completed >= 3 && !this.row_3_completed_seed) this.row_3_completed_seed = plate.seed;
            this.row_1_completed += plate.rows_completed >= 1 ? 1 : 0;
            this.row_2_completed += plate.rows_completed >= 2 ? 1 : 0;
            this.row_3_completed += plate.rows_completed >= 3 ? 1 : 0;
        }
    }

    static removeCheckedNumbers() {
        Board.pickedNumbers.clear();
        Board.check_sum_1_30 = 0;
        Board.check_sum_31_60 = 0;
        Board.check_sum_61_90 = 0;
        Board.row_1_completed = 0;
        Board.row_2_completed = 0;
        Board.row_3_completed = 0;
        Board.row_1_completed_seed = "";
        Board.row_2_completed_seed = "";
        Board.row_3_completed_seed = "";
        for (let i = 0; i < Board.plates.length; i++) {
            let plate = Board.plates[i];
            plate.rows_completed = 0;
        }
    }

    static generatePlates(amount) {
        Plate.generatePlates(amount, Board.plates);
    }
}

function generateCheckSums(numbersList) {
    let check_sums = {
        check_sum_1_30: 0,
        check_sum_31_60: 0,
        check_sum_61_90: 0
    };
    for (let i = 0; i < numbersList.length; i++) {
        let number = numbersList[i];
        if (number >= 1 && number <= 30) {
            check_sums.check_sum_1_30 += 1 << number - 1;
        } else if (number >= 31 && number <= 60) {
            check_sums.check_sum_31_60 += 1 << number - 31;
        } else if (number >= 61 && number <= 90) {
            check_sums.check_sum_61_90 += 1 << number - 61;
        }
    }
    return check_sums;
}

function checkNumber(event) {
    event.preventDefault();
    console.time("pickNumber");
    Board.pickNumber(+document.getElementById('numberInput').value);
    console.timeEnd("pickNumber");
    document.getElementById('numberInput').value = "";
    document.getElementById('pickedNumbersDisplay').innerHTML = Array.from(Board.pickedNumbers).join(", ");
    document.getElementById('row1Completed').innerText = Board.row_1_completed + " (Seed: " + (Board.row_1_completed_seed || "None") + ")";
    document.getElementById('row2Completed').innerText = Board.row_2_completed + " (Seed: " + (Board.row_2_completed_seed || "None") + ")";
    document.getElementById('row3Completed').innerText = Board.row_3_completed + " (Seed: " + (Board.row_3_completed_seed || "None") + ")";
}

function reset() {
    Board.removeCheckedNumbers();
    document.getElementById('pickedNumbersDisplay').innerHTML = "";
    document.getElementById('row1Completed').innerText = "0 (Seed: None)";
    document.getElementById('row2Completed').innerText = "0 (Seed: None)";
    document.getElementById('row3Completed').innerText = "0 (Seed: None)";
}

Board.generatePlates(4_000_000);
console.log(Board.plates);