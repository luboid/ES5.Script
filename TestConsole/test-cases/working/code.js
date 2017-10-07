function test() {
  var ok = false;
  try {
      var o = JSON.stringify;
      var d = delete JSON.stringify;
      if (d === true && JSON.stringify === undefined) {
        ok = true;
      }
  } finally {
    JSON.stringify = o;
  }
  return ok;
}
