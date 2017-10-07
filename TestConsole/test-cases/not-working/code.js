function test() {
  try {
      var o = JSON.stringify;
      var d = delete JSON.stringify;
      if (d === true && JSON.stringify === undefined) {
        return true;
      }
  } finally {
    JSON.stringify = o;
  }
}
