// Copyright 2009 the Sputnik authors.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/**
* @name: S15.10.2.12_A1_T4;
* @section: 15.10.2.12, 7.2, 7.3;
* @assertion: The production CharacterClassEscape :: s evaluates by returning the set of characters 
* containing the characters that are on the right-hand side of the WhiteSpace (7.2) or LineTerminator (7.3) productions;
* @description: RUSSIAN ALPHABET; 
*/

var regexp_s = /\s/;

//CHECK#0410-042F
var result = true;  
for (alpha = 0x0410; alpha <= 0x042F; alpha++) {
  if (regexp_s.exec(String.fromCharCode(alpha)) !== null) {
    result = false;
  }
}

if (result !== true) {
  $ERROR('#1: RUSSIAN CAPITAL ALPHABET');
}

//CHECK#0430-044F
var result = true; 
for (alpha = 0x0430; alpha <= 0x044F; alpha++) {
  if (regexp_s.exec(String.fromCharCode(alpha)) !== null) {
    result = false;
  }
}

if (result !== true) {
  $ERROR('#2: russian small alphabet');
}
