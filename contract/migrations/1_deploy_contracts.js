const HashFix = artifacts.require("HashFix");
const NumberToString = artifacts.require("NumberToString");
const Voting = artifacts.require("Voting");

module.exports = async function(deployer) {
  await deployer.deploy(HashFix);
  await deployer.deploy(NumberToString);

  await deployer.link(HashFix, Voting);
  await deployer.link(NumberToString, Voting);

  await deployer.deploy(Voting);
};
