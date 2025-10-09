/*
 Navicat MySQL Data Transfer

 Source Server         : NMKR Mainnet DigitalOcean
 Source Server Type    : MySQL
 Source Server Version : 80035
 Source Host           : nmkr-studio-mainnet-do-user-17704078-0.h.db.ondigitalocean.com:25060
 Source Schema         : defaultdb

 Target Server Type    : MySQL
 Target Server Version : 80035
 File Encoding         : 65001

 Date: 01/10/2025 16:15:48
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for activeblockchains
-- ----------------------------
DROP TABLE IF EXISTS `activeblockchains`;
CREATE TABLE "activeblockchains" (
  "id" int NOT NULL AUTO_INCREMENT,
  "name" varchar(255) NOT NULL,
  "image" varchar(255) NOT NULL,
  "enabled" tinyint(1) NOT NULL,
  "smallestentity" varchar(255) NOT NULL,
  "coinname" varchar(255) NOT NULL,
  "explorerurladdress" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "explorerurltx" varchar(255) NOT NULL,
  "explorerurlcollection" varchar(255) NOT NULL,
  "factor" bigint NOT NULL,
  "hasnft" tinyint(1) NOT NULL DEFAULT '1',
  "hasft" tinyint(1) NOT NULL DEFAULT '0',
  "collectionmustbecreatedonnewproject" tinyint(1) NOT NULL DEFAULT '0',
  "collectionaddressmustbefunded" tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for adahandles
-- ----------------------------
DROP TABLE IF EXISTS `adahandles`;
CREATE TABLE "adahandles" (
  "id" int NOT NULL AUTO_INCREMENT,
  "policyid" varchar(255) NOT NULL,
  "prefix" varchar(255) NOT NULL,
  "comment" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for adminlogins
-- ----------------------------
DROP TABLE IF EXISTS `adminlogins`;
CREATE TABLE "adminlogins" (
  "id" int NOT NULL AUTO_INCREMENT,
  "email" varchar(255) NOT NULL,
  "password" varchar(255) NOT NULL,
  "salt" varchar(255) NOT NULL,
  "state" enum('active','notactive','blocked','locked','deleted') NOT NULL,
  "created" datetime NOT NULL,
  "ipaddress" varchar(255) NOT NULL,
  "failedlogon" int NOT NULL,
  "twofactor" enum('none','sms','google') NOT NULL,
  "mobilenumber" varchar(255) DEFAULT NULL,
  "lockeduntil" datetime DEFAULT NULL,
  "pendingpassword" varchar(255) DEFAULT NULL,
  "pendingpasswordcreated" datetime DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=215709 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for adminmintandsendaddresses
-- ----------------------------
DROP TABLE IF EXISTS `adminmintandsendaddresses`;
CREATE TABLE "adminmintandsendaddresses" (
  "id" int NOT NULL AUTO_INCREMENT,
  "address" varchar(255) NOT NULL,
  "privateskey" longtext NOT NULL,
  "privatevkey" longtext NOT NULL,
  "lovelace" bigint NOT NULL DEFAULT '0',
  "addressblocked" tinyint(1) NOT NULL DEFAULT '0',
  "blockcounter" int NOT NULL DEFAULT '0',
  "salt" varchar(255) NOT NULL,
  "lasttxhash" varchar(255) DEFAULT NULL,
  "lasttxdate" datetime DEFAULT NULL,
  "reservationtoken" varchar(255) DEFAULT NULL,
  "coin" enum('ADA','SOL','APT','ETH','MATIC','HBAR','BTC') CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT 'ADA',
  "seed" text,
  "lastcheckforutxo" datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=61 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for airdrops
-- ----------------------------
DROP TABLE IF EXISTS `airdrops`;
CREATE TABLE "airdrops" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "created" datetime NOT NULL,
  "mintandsend_id" int DEFAULT NULL,
  "message" varchar(255) DEFAULT NULL,
  "recevieraddress" varchar(255) NOT NULL,
  "uid" varchar(255) NOT NULL,
  "nft_id" int DEFAULT NULL,
  PRIMARY KEY ("id"),
  KEY "airdrops_nftprojects" ("nftproject_id"),
  KEY "airdrops_mintandsend" ("mintandsend_id"),
  KEY "airdrops_uid" ("uid"),
  KEY "airdrops_nfts" ("nft_id"),
  CONSTRAINT "airdrops_mintandsend" FOREIGN KEY ("mintandsend_id") REFERENCES "mintandsend" ("id") ON DELETE SET NULL,
  CONSTRAINT "airdrops_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE CASCADE,
  CONSTRAINT "airdrops_nfts" FOREIGN KEY ("nft_id") REFERENCES "nfts" ("id") ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=98928 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for apikeyaccess
-- ----------------------------
DROP TABLE IF EXISTS `apikeyaccess`;
CREATE TABLE "apikeyaccess" (
  "id" int NOT NULL AUTO_INCREMENT,
  "apikey_id" int NOT NULL,
  "accessfrom" varchar(255) NOT NULL,
  "state" enum('allowed','forbidden') NOT NULL,
  "description" varchar(255) NOT NULL DEFAULT '',
  "order" int NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "apikeyaccess_apikeys" ("apikey_id") USING BTREE,
  CONSTRAINT "apikeyaccess_apikeys" FOREIGN KEY ("apikey_id") REFERENCES "apikeys" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=632 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for apikeys
-- ----------------------------
DROP TABLE IF EXISTS `apikeys`;
CREATE TABLE "apikeys" (
  "id" int NOT NULL AUTO_INCREMENT,
  "customer_id" int NOT NULL DEFAULT '0',
  "apikeyhash" varchar(255) NOT NULL,
  "created" datetime NOT NULL,
  "expiration" datetime NOT NULL,
  "state" enum('active','revoked','deleted') DEFAULT NULL,
  "purchaserandomnft" tinyint(1) DEFAULT NULL,
  "uploadnft" tinyint(1) DEFAULT NULL,
  "listnft" tinyint(1) DEFAULT NULL,
  "makepayouts" tinyint(1) DEFAULT NULL,
  "comment" varchar(255) DEFAULT NULL,
  "purchasespecificnft" tinyint(1) DEFAULT NULL,
  "checkaddresses" tinyint(1) DEFAULT NULL,
  "createprojects" tinyint(1) DEFAULT NULL,
  "apikeystartandend" varchar(255) DEFAULT NULL,
  "listprojects" tinyint(1) DEFAULT NULL,
  "walletvalidation" tinyint(1) DEFAULT NULL,
  "paymenttransactions" tinyint(1) DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  UNIQUE KEY "apikey" ("apikeyhash") USING BTREE,
  KEY "apikeys_customers" ("customer_id") USING BTREE,
  CONSTRAINT "apikeys_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=4867 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for apilogs
-- ----------------------------
DROP TABLE IF EXISTS `apilogs`;
CREATE TABLE "apilogs" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "apifunction" enum('reservenftrandom','reservenftspecific','mintandsend','mintandsign','checkaddress') NOT NULL,
  "year" int NOT NULL,
  "month" int NOT NULL,
  "day" int NOT NULL,
  "hour" int NOT NULL,
  "minute" int NOT NULL,
  "ratelimtexceed" int NOT NULL DEFAULT '0',
  "apicalls" int NOT NULL DEFAULT '0',
  PRIMARY KEY ("id"),
  UNIQUE KEY "apilog1" ("year","month","day","hour","minute","nftproject_id"),
  KEY "apilogs_projects" ("nftproject_id"),
  CONSTRAINT "apilogs_projects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for backgroundserver
-- ----------------------------
DROP TABLE IF EXISTS `backgroundserver`;
CREATE TABLE "backgroundserver" (
  "id" int NOT NULL AUTO_INCREMENT,
  "ipaddress" varchar(255) NOT NULL,
  "url" varchar(255) NOT NULL DEFAULT '',
  "name" varchar(255) NOT NULL,
  "state" enum('active','notactive') DEFAULT NULL,
  "checkpaymentaddresses" tinyint(1) NOT NULL DEFAULT '0',
  "checkdoublepayments" tinyint(1) NOT NULL DEFAULT '0',
  "checkpolicies" tinyint(1) NOT NULL DEFAULT '0',
  "executedatabasecommands" tinyint(1) NOT NULL DEFAULT '0',
  "checkforfreepaymentaddresses" tinyint(1) NOT NULL DEFAULT '0',
  "checkcustomeraddresses" tinyint(1) NOT NULL DEFAULT '0',
  "checkforpremintedaddresses" tinyint(1) NOT NULL DEFAULT '0',
  "executepayoutrequests" tinyint(1) NOT NULL DEFAULT '0',
  "checkfordoublepayments" tinyint(1) NOT NULL DEFAULT '0',
  "checkforexpirednfts" tinyint(1) NOT NULL DEFAULT '0',
  "checkforburningendpoints" tinyint(1) NOT NULL,
  "checkprojectaddresses" tinyint(1) NOT NULL DEFAULT '0',
  "checkmintandsend" tinyint(1) NOT NULL DEFAULT '0',
  "checklegacyauctions" tinyint(1) NOT NULL DEFAULT '0',
  "checklegacydirectsales" tinyint(1) NOT NULL DEFAULT '0',
  "checknotificationqueue" tinyint(1) NOT NULL DEFAULT '0',
  "executesubmissions" tinyint(1) NOT NULL DEFAULT '0',
  "checkdecentralsubmits" tinyint(1) NOT NULL DEFAULT '0',
  "checkroyaltysplitaddresses" tinyint(1) NOT NULL DEFAULT '0',
  "checktransactionconfirmations" tinyint(1) NOT NULL DEFAULT '0',
  "checkbuyinsmartcontractaddresses" tinyint(1) NOT NULL DEFAULT '0',
  "checkcustomerchargeaddresses" tinyint(1) NOT NULL DEFAULT '0',
  "stopserver" tinyint(1) NOT NULL DEFAULT '0',
  "pauseserver" tinyint(1) NOT NULL DEFAULT '0',
  "ratelimitperminute" int NOT NULL DEFAULT '0',
  "mintxcheckdoublepayments" int NOT NULL DEFAULT '10',
  "lastlifesign" datetime NOT NULL,
  "runningversion" varchar(255) NOT NULL,
  "nodeversion" varchar(255) NOT NULL,
  "monitorthisserver" tinyint(1) NOT NULL DEFAULT '0',
  "actualtask" varchar(255) DEFAULT NULL,
  "checkvalidationaddresses" tinyint(1) NOT NULL DEFAULT '0',
  "actualprojectid" int DEFAULT NULL,
  "syncprogress" varchar(255) DEFAULT NULL,
  "epoch" varchar(255) DEFAULT NULL,
  "slot" varchar(255) DEFAULT NULL,
  "block" varchar(255) DEFAULT NULL,
  "era" varchar(255) DEFAULT NULL,
  "operatingsystem" varchar(255) DEFAULT NULL,
  "checkpaymentaddressessolana" tinyint(1) NOT NULL DEFAULT '0',
  "checkmintandsendsolana" tinyint(1) NOT NULL DEFAULT '0',
  "checkpoliciessolana" tinyint(1) NOT NULL DEFAULT '0',
  "installedmemory" varchar(255) DEFAULT NULL,
  "checkcustomeraddressessolana" tinyint(1) NOT NULL DEFAULT '0',
  "checkrates" tinyint(1) NOT NULL DEFAULT '0',
  "checkpaymentaddressescoin" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  "checkmintandsendcoin" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  "checkpoliciescoin" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  "checkcustomeraddressescoin" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  "digitaloceanserver" tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=244 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for backgroundtaskslog
-- ----------------------------
DROP TABLE IF EXISTS `backgroundtaskslog`;
CREATE TABLE "backgroundtaskslog" (
  "id" bigint NOT NULL AUTO_INCREMENT,
  "logmessage" varchar(255) NOT NULL,
  "created" datetime NOT NULL,
  "additionaldata" longtext,
  PRIMARY KEY ("id") USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=1495954927 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for blockedipaddresses
-- ----------------------------
DROP TABLE IF EXISTS `blockedipaddresses`;
CREATE TABLE "blockedipaddresses" (
  "id" int NOT NULL AUTO_INCREMENT,
  "ipaddress" char(50) NOT NULL,
  "blockeduntil" datetime NOT NULL,
  "blockcounter" int NOT NULL,
  PRIMARY KEY ("id"),
  UNIQUE KEY "ipaddress" ("ipaddress")
) ENGINE=InnoDB AUTO_INCREMENT=7466 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for burnigendpoints
-- ----------------------------
DROP TABLE IF EXISTS `burnigendpoints`;
CREATE TABLE "burnigendpoints" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "address" varchar(255) NOT NULL,
  "privateskey" longtext NOT NULL,
  "privatevkey" longtext NOT NULL,
  "lovelace" bigint NOT NULL,
  "salt" varchar(255) NOT NULL,
  "validuntil" datetime NOT NULL,
  "state" enum('active','notactive') NOT NULL DEFAULT 'active',
  "fixnfts" tinyint(1) NOT NULL DEFAULT '0',
  "shownotification" tinyint(1) NOT NULL DEFAULT '0',
  "blockchain" enum('Cardano','Solana') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'Cardano',
  PRIMARY KEY ("id"),
  KEY "buningendpoints_nftprojects" ("nftproject_id"),
  KEY "validuntil" ("validuntil"),
  CONSTRAINT "buningendpoints_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=17182 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for buyoutsmartcontractaddresses
-- ----------------------------
DROP TABLE IF EXISTS `buyoutsmartcontractaddresses`;
CREATE TABLE "buyoutsmartcontractaddresses" (
  "id" int NOT NULL AUTO_INCREMENT,
  "customer_id" int NOT NULL,
  "address" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "skey" text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "vkey" text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "salt" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "lovelace" bigint NOT NULL,
  "state" enum('active','payment_received','finished','expired','error','inprogress','refunded') CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "expiredate" datetime NOT NULL,
  "lockamount" bigint NOT NULL,
  "outgoingtxhash" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  "smartcontracttxhash" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "logfile" text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  "transactionid" varchar(255) NOT NULL,
  "receiveraddress" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  "additionalamount" bigint NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "buyoutsmartcontractaddresses_customers" ("customer_id"),
  CONSTRAINT "buyoutsmartcontractaddresses_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for buyoutsmartcontractaddresses_nfts
-- ----------------------------
DROP TABLE IF EXISTS `buyoutsmartcontractaddresses_nfts`;
CREATE TABLE "buyoutsmartcontractaddresses_nfts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "buyoutsmartcontractaddresses_iid" int NOT NULL,
  "tokennameinhex" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "policyid" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "tokencount" bigint NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "buyoutsmartcontractaddresses" ("buyoutsmartcontractaddresses_iid") USING BTREE,
  CONSTRAINT "buyoutsmartcontractaddresses" FOREIGN KEY ("buyoutsmartcontractaddresses_iid") REFERENCES "buyoutsmartcontractaddresses" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for buyoutsmartcontractaddresses_receivers
-- ----------------------------
DROP TABLE IF EXISTS `buyoutsmartcontractaddresses_receivers`;
CREATE TABLE "buyoutsmartcontractaddresses_receivers" (
  "id" int NOT NULL AUTO_INCREMENT,
  "buyoutsmartcontractaddresses_id" int NOT NULL,
  "receiveraddress" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "lovelace" bigint NOT NULL,
  "pkh" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "buyoutsmartcontractaddresses_2" ("buyoutsmartcontractaddresses_id") USING BTREE,
  CONSTRAINT "buyoutsmartcontractaddresses_2" FOREIGN KEY ("buyoutsmartcontractaddresses_id") REFERENCES "buyoutsmartcontractaddresses" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for countedwhitelist
-- ----------------------------
DROP TABLE IF EXISTS `countedwhitelist`;
CREATE TABLE "countedwhitelist" (
  "id" int NOT NULL AUTO_INCREMENT,
  "saleconditions_id" int NOT NULL,
  "address" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "stakeaddress" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  "maxcount" bigint NOT NULL DEFAULT '1',
  "created" datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY ("id") USING BTREE,
  KEY "countedwhitelist_saleconditions" ("saleconditions_id") USING BTREE,
  CONSTRAINT "countedwhitelist_saleconditions" FOREIGN KEY ("saleconditions_id") REFERENCES "nftprojectsaleconditions" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=735565 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for countedwhitelistusedaddresses
-- ----------------------------
DROP TABLE IF EXISTS `countedwhitelistusedaddresses`;
CREATE TABLE "countedwhitelistusedaddresses" (
  "id" int NOT NULL AUTO_INCREMENT,
  "countedwhitelist_id" int NOT NULL,
  "usedaddress" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "originatoraddress" varchar(255) NOT NULL,
  "transactionid" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "created" datetime NOT NULL,
  "countnft" bigint NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "countedwhitelistusedaddresses_countedwhitelist" ("countedwhitelist_id") USING BTREE,
  CONSTRAINT "countedwhitelistusedaddresses_countedwhitelist" FOREIGN KEY ("countedwhitelist_id") REFERENCES "countedwhitelist" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=32924 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for countries
-- ----------------------------
DROP TABLE IF EXISTS `countries`;
CREATE TABLE "countries" (
  "id" int NOT NULL AUTO_INCREMENT,
  "iso" char(2) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "name" varchar(80) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "nicename" varchar(80) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "iso3" char(3) CHARACTER SET latin1 COLLATE latin1_swedish_ci DEFAULT NULL,
  "numcode" smallint DEFAULT NULL,
  "phonecode" int NOT NULL,
  PRIMARY KEY ("id") USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=242 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for counttotal
-- ----------------------------
DROP TABLE IF EXISTS `counttotal`;
CREATE TABLE "counttotal" (
  "counttotal" int NOT NULL,
  PRIMARY KEY ("counttotal") USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for custodialwallets
-- ----------------------------
DROP TABLE IF EXISTS `custodialwallets`;
CREATE TABLE "custodialwallets" (
  "id" int NOT NULL AUTO_INCREMENT,
  "uid" varchar(255) NOT NULL,
  "customer_id" int NOT NULL,
  "walletname" varchar(255) NOT NULL,
  "address" varchar(255) NOT NULL,
  "skey" text NOT NULL,
  "vkey" text NOT NULL,
  "seedphrase" text NOT NULL,
  "salt" varchar(255) NOT NULL,
  "wallettype" enum('enterprise','base') CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "created" datetime NOT NULL,
  "lastcheckforutxo" datetime DEFAULT NULL,
  "state" enum('active','blocked','notactive','deleted') CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "pincode" varchar(255) NOT NULL,
  PRIMARY KEY ("id"),
  KEY "custodialwallets_customers" ("customer_id"),
  CONSTRAINT "custodialwallets_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=834 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for customeraddresses
-- ----------------------------
DROP TABLE IF EXISTS `customeraddresses`;
CREATE TABLE "customeraddresses" (
  "id" int NOT NULL AUTO_INCREMENT,
  "customer_id" int NOT NULL,
  "address" varchar(255) NOT NULL,
  "seedphrase" text,
  "vkey" text,
  "skey" text,
  "blockchain" enum('Cardano','Solana') DEFAULT NULL,
  "state" enum('active','notactive') DEFAULT NULL,
  "lastchecked" datetime DEFAULT NULL,
  "salt" varchar(255) NOT NULL,
  PRIMARY KEY ("id"),
  KEY "customeraddresses_customers" ("customer_id"),
  CONSTRAINT "customeraddresses_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for customerlogins
-- ----------------------------
DROP TABLE IF EXISTS `customerlogins`;
CREATE TABLE "customerlogins" (
  "id" int NOT NULL AUTO_INCREMENT,
  "customer_id" int NOT NULL,
  "created" datetime NOT NULL,
  "ipaddress" varchar(255) NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "customerlogins_customers" ("customer_id") USING BTREE,
  CONSTRAINT "customerlogins_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=63540 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for customers
-- ----------------------------
DROP TABLE IF EXISTS `customers`;
CREATE TABLE "customers" (
  "id" int NOT NULL AUTO_INCREMENT,
  "email" varchar(255) NOT NULL,
  "password" varchar(255) NOT NULL,
  "salt" varchar(255) NOT NULL,
  "company" varchar(255) DEFAULT NULL,
  "firstname" varchar(255) DEFAULT NULL,
  "lastname" varchar(255) DEFAULT NULL,
  "street" varchar(255) DEFAULT NULL,
  "zip" varchar(8) DEFAULT NULL,
  "city" varchar(255) DEFAULT NULL,
  "country_id" int NOT NULL,
  "ustid" varchar(255) DEFAULT NULL,
  "confirmationcode" varchar(255) DEFAULT NULL,
  "state" enum('active','notactive','blocked','locked','deleted') NOT NULL,
  "created" datetime NOT NULL,
  "ipaddress" varchar(255) NOT NULL,
  "failedlogon" int NOT NULL,
  "twofactor" enum('none','sms','google') NOT NULL,
  "mobilenumber" varchar(255) DEFAULT NULL,
  "lockeduntil" datetime DEFAULT NULL,
  "avatarid" int NOT NULL,
  "pendingpassword" varchar(255) DEFAULT NULL,
  "pendingpasswordcreated" datetime DEFAULT NULL,
  "sendmailonlogon" tinyint(1) NOT NULL,
  "sendmailonlogonfailure" tinyint(1) NOT NULL,
  "sendmailonpayout" tinyint(1) NOT NULL,
  "sendmailonnews" tinyint(1) NOT NULL,
  "sendmailonservice" tinyint(1) NOT NULL,
  "adaaddress" varchar(255) DEFAULT NULL,
  "privatevkey" longtext,
  "privateskey" longtext,
  "lovelace" bigint DEFAULT NULL,
  "addressblocked" tinyint(1) NOT NULL DEFAULT '0',
  "blockcounter" int NOT NULL DEFAULT '0',
  "sendmailonsale" tinyint(1) NOT NULL DEFAULT '0',
  "defaultsettings_id" int NOT NULL DEFAULT '3',
  "marketplacesettings_id" int NOT NULL DEFAULT '1',
  "checkaddressalways" tinyint(1) NOT NULL DEFAULT '0',
  "ftppassword" varchar(255) DEFAULT NULL,
  "referal" varchar(255) DEFAULT NULL,
  "checkaddresscount" int NOT NULL DEFAULT '0',
  "lastcheckforutxo" datetime DEFAULT NULL,
  "comments" longtext,
  "twofactorenabled" datetime DEFAULT NULL,
  "kycaccesstoken" varchar(255) DEFAULT NULL,
  "kycprocessed" datetime DEFAULT NULL,
  "kycstatus" varchar(255) DEFAULT NULL,
  "checkkycstate" enum('always','untilgreen','never') NOT NULL DEFAULT 'untilgreen',
  "showkycstate" tinyint(1) NOT NULL DEFAULT '1',
  "showpayoutbutton" tinyint(1) NOT NULL DEFAULT '0',
  "kycresultmessage" longtext,
  "splitroyaltyaddressespercentage" int NOT NULL DEFAULT '200' COMMENT '200 = 2 percent',
  "purchasedmints" int NOT NULL DEFAULT '0' COMMENT 'Will not used anymore - use newpurchasedmints now',
  "defaultpromotion_id" int DEFAULT NULL,
  "lasttxhash" varchar(255) DEFAULT NULL,
  "stakevkey" text,
  "stakeskey" text,
  "internalaccount" tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Mark if this account is uses as internal account',
  "chargemintandsendcostslovelace" bigint NOT NULL DEFAULT '0',
  "connectedwallettype" varchar(255) DEFAULT NULL,
  "connectedwalletchangeaddress" varchar(255) DEFAULT NULL,
  "donotneedtolocktokens" tinyint(1) NOT NULL DEFAULT '0',
  "kycprovider" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL COMMENT 'Yoti or IAMX',
  "newpurchasedmints" float(12,2) NOT NULL DEFAULT '0.00' COMMENT 'New Mint Coupons with comma. So we can have also 0,5 mint coupons for an update',
  "solanaseedphrase" text,
  "solanapublickey" varchar(255) DEFAULT NULL,
  "lamports" bigint NOT NULL DEFAULT '0',
  "soladdressblocked" tinyint(1) NOT NULL DEFAULT '0',
  "sollastcheckforutxo" datetime DEFAULT NULL,
  "subcustomer_id" int DEFAULT NULL,
  "subcustomerdescription" varchar(255) DEFAULT NULL,
  "subcustomerexternalid" varchar(255) DEFAULT NULL,
  "aptosaddress" varchar(255) DEFAULT NULL,
  "aptosprivatekey" varchar(255) DEFAULT NULL,
  "aptosseed" varchar(255) DEFAULT NULL,
  "aptaddressblocked" tinyint(1) NOT NULL DEFAULT '0',
  "aptlastcheckforutxo" datetime DEFAULT NULL,
  "octas" bigint NOT NULL DEFAULT '0',
  "two2falogin" tinyint(1) NOT NULL DEFAULT '1',
  "two2facreatewallet" tinyint(1) NOT NULL DEFAULT '1',
  "two2faexportkeys" tinyint(1) NOT NULL DEFAULT '1',
  "two2fapaymentsmanagedwallets" tinyint(1) NOT NULL DEFAULT '1',
  "two2fadeleteprojects" tinyint(1) NOT NULL DEFAULT '1',
  "two2facreateapikey" tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY ("id") USING BTREE,
  KEY "customers_countries" ("country_id") USING BTREE,
  KEY "customers_settings" ("defaultsettings_id"),
  KEY "customers_marketplacesettings" ("marketplacesettings_id"),
  KEY "customers_promotions" ("defaultpromotion_id"),
  KEY "customers_customers" ("subcustomer_id"),
  CONSTRAINT "customers_countries" FOREIGN KEY ("country_id") REFERENCES "countries" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "customers_customers" FOREIGN KEY ("subcustomer_id") REFERENCES "customers" ("id") ON DELETE RESTRICT,
  CONSTRAINT "customers_marketplacesettings" FOREIGN KEY ("marketplacesettings_id") REFERENCES "smartcontractsmarketplacesettings" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "customers_promotions" FOREIGN KEY ("defaultpromotion_id") REFERENCES "promotions" ("id") ON DELETE SET NULL,
  CONSTRAINT "customers_settings" FOREIGN KEY ("defaultsettings_id") REFERENCES "settings" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=223365 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for customerwallets
-- ----------------------------
DROP TABLE IF EXISTS `customerwallets`;
CREATE TABLE "customerwallets" (
  "id" int NOT NULL AUTO_INCREMENT,
  "customer_id" int NOT NULL,
  "walletaddress" varchar(255) NOT NULL,
  "created" datetime NOT NULL,
  "state" enum('active','deleted','notactive') NOT NULL,
  "ipaddress" varchar(255) DEFAULT NULL,
  "comment" varchar(255) DEFAULT NULL,
  "confirmationcode" varchar(255) DEFAULT NULL,
  "confirmationvalid" datetime DEFAULT NULL,
  "hash" varchar(255) DEFAULT NULL,
  "confirmationdate" datetime DEFAULT NULL,
  "cointype" enum('ADA','ETH','USDC','SOL','APT','HBAR','BTC') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'ADA',
  PRIMARY KEY ("id") USING BTREE,
  KEY "customerwallets_customers" ("customer_id") USING BTREE,
  CONSTRAINT "customerwallets_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=13160 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for defaulttemplates
-- ----------------------------
DROP TABLE IF EXISTS `defaulttemplates`;
CREATE TABLE "defaulttemplates" (
  "id" int NOT NULL AUTO_INCREMENT,
  "template" longtext NOT NULL,
  "description" varchar(255) NOT NULL,
  PRIMARY KEY ("id") USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for digitalidentities
-- ----------------------------
DROP TABLE IF EXISTS `digitalidentities`;
CREATE TABLE "digitalidentities" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "policyid" varchar(255) NOT NULL,
  "created" datetime NOT NULL,
  "state" enum('active','notactive','expired','tokencreated','didresultreceived','canceled','error') CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "didprovider" enum('NMKR','IAMX') NOT NULL,
  "didjsonresult" text,
  "didresultreceived" datetime DEFAULT NULL,
  "tokenjson" text,
  "resultmessage" text,
  "ipfshash" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id"),
  KEY "digitalidentities_projects" ("nftproject_id"),
  KEY "digitalidentities_policyid" ("policyid"),
  CONSTRAINT "digitalidentities_projects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=534 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for directsales
-- ----------------------------
DROP TABLE IF EXISTS `directsales`;
CREATE TABLE "directsales" (
  "id" int NOT NULL AUTO_INCREMENT,
  "smartcontract_id" int NOT NULL,
  "transactionid" varchar(255) NOT NULL,
  "nftproject_id" int DEFAULT NULL,
  "customer_id" int DEFAULT NULL,
  "price" bigint NOT NULL,
  "selleraddress" varchar(255) DEFAULT NULL,
  "buyer" varchar(255) DEFAULT NULL,
  "created" datetime NOT NULL,
  "state" enum('deleted','prepared','waitingforbid','sold','canceled','readytosignbyseller','readytosignbybuyer','auctionexpired','waitingforsale','waitingforlocknft','submitted','confirmed','readytosignbysellercancel') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "royaltyfeespercent" float(12,2) DEFAULT NULL,
  "royaltyaddress" varchar(255) DEFAULT NULL,
  "marketplacefeepercent" float(12,2) DEFAULT NULL,
  "nmkrfeepercent" float(12,2) DEFAULT NULL,
  "refererfeepercent" float(12,2) DEFAULT NULL,
  "locknftstxinhashid" varchar(255) DEFAULT NULL,
  "solddate" datetime DEFAULT NULL,
  "lockamount" bigint NOT NULL DEFAULT '2000000',
  "name" varchar(255) NOT NULL,
  "nmkrpaylink" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "directsales_nftprojects" ("nftproject_id") USING BTREE,
  KEY "directsales_customers" ("customer_id") USING BTREE,
  KEY "directsales_smartcontracts" ("smartcontract_id") USING BTREE,
  CONSTRAINT "directsales_ibfk_1" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT "directsales_ibfk_2" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT "directsales_ibfk_3" FOREIGN KEY ("smartcontract_id") REFERENCES "smartcontracts" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=201 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for directsales_nfts
-- ----------------------------
DROP TABLE IF EXISTS `directsales_nfts`;
CREATE TABLE "directsales_nfts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "directsale_id" int NOT NULL,
  "policyid" varchar(255) NOT NULL,
  "tokennamehex" varchar(255) NOT NULL,
  "ipfshash" varchar(255) NOT NULL,
  "metadata" text,
  "tokencount" bigint NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "directsales_nfts_directsales" ("directsale_id") USING BTREE,
  CONSTRAINT "directsales_nfts_directsales" FOREIGN KEY ("directsale_id") REFERENCES "directsales" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=201 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for emailtemplates
-- ----------------------------
DROP TABLE IF EXISTS `emailtemplates`;
CREATE TABLE "emailtemplates" (
  "id" int NOT NULL AUTO_INCREMENT,
  "templatename" varchar(255) NOT NULL,
  "language" varchar(2) NOT NULL,
  "textemail" longtext,
  "htmlemail" longtext,
  "emailsubject" varchar(255) NOT NULL,
  PRIMARY KEY ("id") USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for faq
-- ----------------------------
DROP TABLE IF EXISTS `faq`;
CREATE TABLE "faq" (
  "id" int NOT NULL AUTO_INCREMENT,
  "question" longtext NOT NULL,
  "answer" longtext NOT NULL,
  "language" varchar(2) NOT NULL,
  "state" enum('active','notactive') NOT NULL,
  "faqcategory_id" int NOT NULL,
  PRIMARY KEY ("id"),
  KEY "faq_faqcategories" ("faqcategory_id"),
  CONSTRAINT "faq_faqcategories" FOREIGN KEY ("faqcategory_id") REFERENCES "faqcategories" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for faqcategories
-- ----------------------------
DROP TABLE IF EXISTS `faqcategories`;
CREATE TABLE "faqcategories" (
  "id" int NOT NULL AUTO_INCREMENT,
  "categoryname" varchar(255) NOT NULL,
  "language" varchar(2) NOT NULL,
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for getaccesstokensuser
-- ----------------------------
DROP TABLE IF EXISTS `getaccesstokensuser`;
CREATE TABLE "getaccesstokensuser" (
  "id" int NOT NULL AUTO_INCREMENT,
  "friendlyname" varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "secret" varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "state" enum('active','notactive') CHARACTER SET latin1 COLLATE latin1_swedish_ci DEFAULT NULL,
  "customer_id" int DEFAULT NULL,
  PRIMARY KEY ("id"),
  UNIQUE KEY "secret" ("secret"),
  KEY "getaccesstokensuser_customers" ("customer_id"),
  CONSTRAINT "getaccesstokensuser_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for gettokensipaddresses
-- ----------------------------
DROP TABLE IF EXISTS `gettokensipaddresses`;
CREATE TABLE "gettokensipaddresses" (
  "id" int NOT NULL AUTO_INCREMENT,
  "ipaddress" varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "friendlyname" varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for informationtexts
-- ----------------------------
DROP TABLE IF EXISTS `informationtexts`;
CREATE TABLE "informationtexts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "informationtext" longtext NOT NULL,
  "created" datetime NOT NULL,
  "state" enum('active','notactive') DEFAULT NULL,
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for invoicedetails
-- ----------------------------
DROP TABLE IF EXISTS `invoicedetails`;
CREATE TABLE "invoicedetails" (
  "id" int NOT NULL AUTO_INCREMENT,
  "invoice_id" int NOT NULL,
  "description" varchar(255) NOT NULL,
  "count" int NOT NULL,
  "pricesingleada" double(20,6) NOT NULL,
  "pricesingleeur" double(20,2) NOT NULL,
  "pricetotalada" double(20,6) NOT NULL,
  "pricetotaleur" double(20,2) NOT NULL,
  "averageadarate" double(20,6) NOT NULL,
  "mintcostsada" double(20,6) NOT NULL DEFAULT '0.000000',
  "mintcostseur" double(20,6) NOT NULL DEFAULT '0.000000',
  PRIMARY KEY ("id"),
  KEY "invoicedetails" ("invoice_id"),
  CONSTRAINT "invoicedetails" FOREIGN KEY ("invoice_id") REFERENCES "invoices" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=226063 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for invoices
-- ----------------------------
DROP TABLE IF EXISTS `invoices`;
CREATE TABLE "invoices" (
  "id" int NOT NULL AUTO_INCREMENT,
  "invoiceno" int NOT NULL,
  "customer_id" int NOT NULL,
  "company" varchar(255) DEFAULT NULL,
  "firstname" varchar(255) NOT NULL,
  "lastname" varchar(255) NOT NULL,
  "street" varchar(255) NOT NULL,
  "zip" varchar(255) NOT NULL,
  "city" varchar(255) NOT NULL,
  "country_id" int NOT NULL,
  "ustid" varchar(255) DEFAULT NULL,
  "invoicedate" datetime NOT NULL,
  "adarate" double(20,6) NOT NULL,
  "netada" double(20,6) NOT NULL,
  "neteur" double(20,2) NOT NULL,
  "usteur" double(20,2) NOT NULL,
  "grosseur" double(20,2) NOT NULL,
  "discounteur" double(20,2) NOT NULL,
  "discountpercent" double(12,2) NOT NULL,
  "billingperiod" varchar(255) DEFAULT NULL,
  "taxrate" double(12,2) NOT NULL,
  PRIMARY KEY ("id"),
  KEY "invoices_customers" ("customer_id"),
  KEY "invoices_countries" ("country_id"),
  CONSTRAINT "invoices_countries" FOREIGN KEY ("country_id") REFERENCES "countries" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "invoices_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=14635 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for ip2location_db11
-- ----------------------------
DROP TABLE IF EXISTS `ip2location_db11`;
CREATE TABLE "ip2location_db11" (
  "ip_from" int unsigned NOT NULL,
  "ip_to" int unsigned NOT NULL,
  "country_code" char(2) CHARACTER SET utf8mb3 COLLATE utf8mb3_bin DEFAULT NULL,
  "country_name" varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_bin DEFAULT NULL,
  "region_name" varchar(128) CHARACTER SET utf8mb3 COLLATE utf8mb3_bin DEFAULT NULL,
  "city_name" varchar(128) CHARACTER SET utf8mb3 COLLATE utf8mb3_bin DEFAULT NULL,
  "latitude" double DEFAULT NULL,
  "longitude" double DEFAULT NULL,
  "zip_code" varchar(30) CHARACTER SET utf8mb3 COLLATE utf8mb3_bin DEFAULT NULL,
  "time_zone" varchar(8) CHARACTER SET utf8mb3 COLLATE utf8mb3_bin DEFAULT NULL,
  PRIMARY KEY ("ip_from","ip_to"),
  KEY "idx_ip_from" ("ip_from"),
  KEY "idx_ip_to" ("ip_to"),
  KEY "idx_ip_from_to" ("ip_from","ip_to")
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_bin;

-- ----------------------------
-- Table structure for ipfsuploads
-- ----------------------------
DROP TABLE IF EXISTS `ipfsuploads`;
CREATE TABLE "ipfsuploads" (
  "id" int NOT NULL AUTO_INCREMENT,
  "customer_id" int NOT NULL,
  "created" datetime NOT NULL,
  "ipfshash" varchar(255) NOT NULL,
  "mimetype" varchar(255) NOT NULL,
  "name" varchar(255) NOT NULL,
  "filesize" bigint NOT NULL DEFAULT '0',
  PRIMARY KEY ("id"),
  KEY "ipfsuploads_customers" ("customer_id"),
  KEY "ipfsuploads_ipfshash" ("ipfshash","customer_id"),
  CONSTRAINT "ipfsuploads_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=32956 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for kycmedia
-- ----------------------------
DROP TABLE IF EXISTS `kycmedia`;
CREATE TABLE "kycmedia" (
  "id" int NOT NULL AUTO_INCREMENT,
  "customer_id" int NOT NULL,
  "mimetype" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "documenttype" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "base64uri" longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci,
  "content" binary(1) DEFAULT NULL,
  "contenttext" text,
  PRIMARY KEY ("id") USING BTREE,
  KEY "kycmedia_customers" ("customer_id") USING BTREE,
  CONSTRAINT "kycmedia_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=16646 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for legacyauctions
-- ----------------------------
DROP TABLE IF EXISTS `legacyauctions`;
CREATE TABLE "legacyauctions" (
  "id" int NOT NULL AUTO_INCREMENT,
  "auctionname" varchar(255) DEFAULT NULL,
  "address" varchar(255) NOT NULL,
  "skey" text NOT NULL,
  "vkey" text NOT NULL,
  "salt" varchar(255) NOT NULL,
  "nftproject_id" int DEFAULT NULL,
  "customer_id" int DEFAULT NULL,
  "minbet" bigint NOT NULL,
  "actualbet" bigint NOT NULL,
  "runsuntil" datetime NOT NULL,
  "selleraddress" varchar(255) DEFAULT NULL,
  "highestbidder" varchar(255) DEFAULT NULL,
  "created" datetime NOT NULL,
  "state" enum('active','notactive','finished','ended','deleted','canceled','waitforlock') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Active=Address will monitored, Notactive=Adress not monitored, Finished=Auction finnished, but still monitoring, Ended=Auction finished, not any longer monitoring',
  "royaltyfeespercent" float(12,2) DEFAULT NULL,
  "royaltyaddress" varchar(255) DEFAULT NULL,
  "marketplacefeepercent" float(12,2) DEFAULT NULL,
  "locknftstxinhashid" varchar(255) DEFAULT NULL,
  "uid" varchar(32) DEFAULT NULL,
  "log" text,
  PRIMARY KEY ("id") USING BTREE,
  UNIQUE KEY "legacyauctions_uid" ("uid"),
  KEY "legacyauctions_nftprojects" ("nftproject_id") USING BTREE,
  KEY "legacyauctions_customers" ("customer_id"),
  CONSTRAINT "legacyauctions_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT "legacyauctions_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE SET NULL ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=391 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for legacyauctions_nfts
-- ----------------------------
DROP TABLE IF EXISTS `legacyauctions_nfts`;
CREATE TABLE "legacyauctions_nfts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "legacyauction_id" int NOT NULL,
  "policyid" varchar(255) NOT NULL,
  "tokennamehex" varchar(255) NOT NULL,
  "ipfshash" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '',
  "metadata" text,
  "tokencount" bigint NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "legaceauctionsnfts_legacyauctions" ("legacyauction_id") USING BTREE,
  CONSTRAINT "legaceauctionsnfts_legacyauctions" FOREIGN KEY ("legacyauction_id") REFERENCES "legacyauctions" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=126 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for legacyauctionshistory
-- ----------------------------
DROP TABLE IF EXISTS `legacyauctionshistory`;
CREATE TABLE "legacyauctionshistory" (
  "id" int NOT NULL AUTO_INCREMENT,
  "legacyauction_id" int NOT NULL,
  "txhash" varchar(255) NOT NULL,
  "senderaddress" varchar(255) NOT NULL,
  "bidamount" bigint NOT NULL,
  "created" datetime NOT NULL,
  "state" enum('seller','outbid','invalid','expired','buyer') NOT NULL,
  "returntxhash" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id"),
  KEY "legacyauctionshistory_legcyauctions" ("legacyauction_id"),
  CONSTRAINT "legacyauctionshistory_legcyauctions" FOREIGN KEY ("legacyauction_id") REFERENCES "legacyauctions" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=415 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for legacydirectsales
-- ----------------------------
DROP TABLE IF EXISTS `legacydirectsales`;
CREATE TABLE "legacydirectsales" (
  "id" int NOT NULL AUTO_INCREMENT,
  "address" varchar(255) NOT NULL,
  "skey" text NOT NULL,
  "vkey" text NOT NULL,
  "salt" varchar(255) NOT NULL,
  "nftproject_id" int DEFAULT NULL,
  "customer_id" int DEFAULT NULL,
  "price" bigint NOT NULL,
  "selleraddress" varchar(255) DEFAULT NULL,
  "buyer" varchar(255) DEFAULT NULL,
  "created" datetime NOT NULL,
  "state" enum('active','notactive','finished','ended') NOT NULL COMMENT 'Active=Address will monitored, Notactive=Adress not monitored, Finished=Auction finnished, but still monitoring, Ended=Auction finished, not any longer monitoring',
  "royaltyfeespercent" float(12,2) DEFAULT NULL,
  "royaltyaddress" varchar(255) DEFAULT NULL,
  "marketplacefeepercent" float(12,2) DEFAULT NULL,
  "locknftstxinhashid" varchar(255) DEFAULT NULL,
  "solddate" datetime DEFAULT NULL,
  "lockamount" bigint NOT NULL DEFAULT '2000000',
  PRIMARY KEY ("id") USING BTREE,
  KEY "legacyauctions_nftprojects" ("nftproject_id") USING BTREE,
  KEY "legacyauctions_customers" ("customer_id") USING BTREE,
  CONSTRAINT "legacydirectsales_ibfk_1" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT "legacydirectsales_ibfk_2" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE SET NULL ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for legacydirectsales_nfts
-- ----------------------------
DROP TABLE IF EXISTS `legacydirectsales_nfts`;
CREATE TABLE "legacydirectsales_nfts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "legacydirectsale_id" int NOT NULL,
  "policyid" varchar(255) NOT NULL,
  "tokennamehex" varchar(255) NOT NULL,
  "ipfshash" varchar(255) NOT NULL,
  "metadata" text,
  "tokencount" bigint NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "legacydirectsales_nfts_legacy_directsales" ("legacydirectsale_id") USING BTREE,
  CONSTRAINT "legacydirectsales_nfts_legacy_directsales" FOREIGN KEY ("legacydirectsale_id") REFERENCES "legacydirectsales" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for lockedassets
-- ----------------------------
DROP TABLE IF EXISTS `lockedassets`;
CREATE TABLE "lockedassets" (
  "id" int NOT NULL AUTO_INCREMENT,
  "customer_id" int NOT NULL,
  "created" datetime NOT NULL,
  "changeaddress" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "lockwalletpkh" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "lockassetaddress" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "lovelace" bigint NOT NULL,
  "lockeduntil" datetime NOT NULL,
  "policyscript" text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  "locktxid" varchar(255) NOT NULL,
  "unlocked" datetime DEFAULT NULL,
  "unlocktxid" varchar(255) DEFAULT NULL,
  "walletname" varchar(255) DEFAULT NULL,
  "confirmedlock" tinyint(1) DEFAULT NULL,
  "confirmedunlock" tinyint(1) DEFAULT NULL,
  "lockslot" bigint NOT NULL,
  "description" varchar(255) DEFAULT NULL,
  "state" enum('active','deleted') NOT NULL DEFAULT 'active',
  PRIMARY KEY ("id") USING BTREE,
  KEY "lockedassets_customers" ("customer_id") USING BTREE,
  CONSTRAINT "lockedassets_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=2223 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for lockedassetstokens
-- ----------------------------
DROP TABLE IF EXISTS `lockedassetstokens`;
CREATE TABLE "lockedassetstokens" (
  "id" int NOT NULL AUTO_INCREMENT,
  "lockedassets_id" int NOT NULL,
  "policyid" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "tokennameinhex" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "count" bigint NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "lockedassetstokens_lockedassets" ("lockedassets_id") USING BTREE,
  CONSTRAINT "lockedassetstokens_lockedassets" FOREIGN KEY ("lockedassets_id") REFERENCES "lockedassets" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for log
-- ----------------------------
DROP TABLE IF EXISTS `log`;
CREATE TABLE "log" (
  "id" int NOT NULL AUTO_INCREMENT,
  "logtext" varchar(255) NOT NULL,
  "created" datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "ipaddress" varchar(255) NOT NULL,
  PRIMARY KEY ("id") USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for loggedinhashes
-- ----------------------------
DROP TABLE IF EXISTS `loggedinhashes`;
CREATE TABLE "loggedinhashes" (
  "id" int NOT NULL AUTO_INCREMENT,
  "customer_id" int NOT NULL,
  "hash" varchar(255) NOT NULL,
  "ipaddress" varchar(255) NOT NULL,
  "validuntil" datetime NOT NULL,
  "lastlifesign" datetime DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "loggedinhashes" ("hash","ipaddress") USING BTREE,
  KEY "loggedinhashes_customers" ("customer_id") USING BTREE,
  CONSTRAINT "loggedinhashes_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=245674 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for manualrefunds
-- ----------------------------
DROP TABLE IF EXISTS `manualrefunds`;
CREATE TABLE "manualrefunds" (
  "id" int NOT NULL AUTO_INCREMENT,
  "txin" varchar(255) NOT NULL,
  "lovelace" bigint NOT NULL,
  "senderaddress" varchar(255) NOT NULL,
  "sendout" tinyint(1) NOT NULL DEFAULT '0',
  "txindate" datetime NOT NULL,
  "transactionid" varchar(255) DEFAULT NULL,
  "log" longtext,
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=5229 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for metadata
-- ----------------------------
DROP TABLE IF EXISTS `metadata`;
CREATE TABLE "metadata" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nft_id" int NOT NULL,
  "placeholdername" varchar(255) DEFAULT NULL,
  "placeholdervalue" longtext,
  PRIMARY KEY ("id") USING BTREE,
  KEY "metadata_nft" ("nft_id"),
  CONSTRAINT "metadata_nft" FOREIGN KEY ("nft_id") REFERENCES "nfts" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=44234865 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for metadatafields
-- ----------------------------
DROP TABLE IF EXISTS `metadatafields`;
CREATE TABLE "metadatafields" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "metadataname" varchar(255) NOT NULL,
  "metadatatype" enum('string','arrayofstring','int','arrayofint','ipfslink','sha256hash','mediatype') NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "metadatafields_nftprojects" ("nftproject_id") USING BTREE,
  CONSTRAINT "metadatafields_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for metadatatemplate
-- ----------------------------
DROP TABLE IF EXISTS `metadatatemplate`;
CREATE TABLE "metadatatemplate" (
  "id" int NOT NULL AUTO_INCREMENT,
  "metadatatemplate" longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "title" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "logo" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  "state" enum('active','notactive') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "description" text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci,
  "projecttype" enum('nft','ft','misc') NOT NULL DEFAULT 'nft',
  PRIMARY KEY ("id") USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for metadatatemplateadditionalfiles
-- ----------------------------
DROP TABLE IF EXISTS `metadatatemplateadditionalfiles`;
CREATE TABLE "metadatatemplateadditionalfiles" (
  "id" int NOT NULL AUTO_INCREMENT,
  "metadatatemplate_id" int NOT NULL,
  "filename" varchar(255) NOT NULL,
  "filetypes" varchar(255) NOT NULL,
  PRIMARY KEY ("id"),
  KEY "metadatatemplateadditionalfiles_metadatatemplates" ("metadatatemplate_id"),
  CONSTRAINT "metadatatemplateadditionalfiles_metadatatemplates" FOREIGN KEY ("metadatatemplate_id") REFERENCES "metadatatemplate" ("id") ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for mimetypes
-- ----------------------------
DROP TABLE IF EXISTS `mimetypes`;
CREATE TABLE "mimetypes" (
  "id" int NOT NULL AUTO_INCREMENT,
  "mimetype" varchar(255) NOT NULL,
  "fileextensions" varchar(255) NOT NULL,
  "allowedasmain" tinyint(1) NOT NULL,
  "placeholderfile" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for mintandsend
-- ----------------------------
DROP TABLE IF EXISTS `mintandsend`;
CREATE TABLE "mintandsend" (
  "id" int NOT NULL AUTO_INCREMENT,
  "customer_id" int NOT NULL,
  "nftproject_id" int NOT NULL,
  "receiveraddress" varchar(255) NOT NULL,
  "created" datetime NOT NULL,
  "state" enum('execute','success','error','canceled','inprogress') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "reservationtoken" varchar(255) NOT NULL,
  "executed" datetime DEFAULT NULL,
  "transactionid" varchar(255) DEFAULT NULL,
  "reservelovelace" bigint NOT NULL DEFAULT '0',
  "onlinenotification" tinyint(1) NOT NULL DEFAULT '0',
  "buildtransaction" longtext,
  "usecustomerwallet" tinyint(1) DEFAULT '1',
  "remintandburn" tinyint(1) NOT NULL DEFAULT '0',
  "confirmed" tinyint(1) NOT NULL DEFAULT '0',
  "coin" enum('ADA','SOL','APT','HBAR','MATIC','SONY','BTC') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'ADA',
  "retry" int NOT NULL DEFAULT '0',
  "requiredcoupons" float NOT NULL DEFAULT '1',
  PRIMARY KEY ("id"),
  KEY "mintandsend_nftprojects" ("nftproject_id"),
  KEY "mintandsendstate" ("customer_id","state"),
  KEY "state" ("state"),
  KEY "reservationtoken" ("reservationtoken"),
  KEY "receiveraddress" ("receiveraddress"),
  KEY "transactionid" ("transactionid"),
  CONSTRAINT "mintandsend_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "mintandsend_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=332004 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for newrates
-- ----------------------------
DROP TABLE IF EXISTS `newrates`;
CREATE TABLE "newrates" (
  "id" int NOT NULL AUTO_INCREMENT,
  "coin" varchar(10) NOT NULL,
  "currency" enum('EUR','USD','JPY','BTC') NOT NULL,
  "effectivedate" datetime NOT NULL,
  "price" double(20,10) DEFAULT NULL,
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=1509438 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for nftaddresses
-- ----------------------------
DROP TABLE IF EXISTS `nftaddresses`;
CREATE TABLE "nftaddresses" (
  "id" int NOT NULL AUTO_INCREMENT,
  "address" varchar(255) NOT NULL,
  "expires" datetime NOT NULL,
  "price" bigint DEFAULT NULL,
  "privateskey" longtext,
  "privatevkey" longtext,
  "state" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  "created" datetime NOT NULL,
  "lovelace" bigint DEFAULT NULL,
  "nftproject_id" int DEFAULT NULL,
  "txid" varchar(255) DEFAULT NULL,
  "senderaddress" varchar(255) DEFAULT NULL,
  "paydate" datetime DEFAULT NULL,
  "salt" varchar(255) DEFAULT NULL,
  "utxo" bigint NOT NULL DEFAULT '0',
  "lastcheckforutxo" datetime DEFAULT NULL,
  "errormessage" varchar(255) DEFAULT NULL,
  "submissionresult" longtext,
  "countnft" bigint DEFAULT NULL,
  "reservationtype" enum('random','specific') DEFAULT NULL,
  "checkonlybyserverid" int DEFAULT NULL,
  "tokencount" bigint NOT NULL DEFAULT '1' COMMENT 'not used at the moment',
  "reservationtoken" varchar(255) DEFAULT NULL,
  "serverid" int DEFAULT NULL,
  "addresscheckedcounter" int NOT NULL DEFAULT '0',
  "checkfordoublepayment" tinyint(1) NOT NULL DEFAULT '0',
  "rejectreason" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  "rejectparameter" varchar(255) DEFAULT NULL,
  "ipaddress" varchar(40) DEFAULT NULL,
  "stakereward" bigint DEFAULT NULL,
  "priceintoken" bigint DEFAULT NULL,
  "tokenpolicyid" varchar(255) DEFAULT NULL,
  "tokenassetid" varchar(255) DEFAULT NULL,
  "tokenmultiplier" bigint NOT NULL DEFAULT '1',
  "foundinslot" bigint DEFAULT NULL,
  "discount" bigint DEFAULT NULL,
  "sendbacktouser" bigint NOT NULL DEFAULT '0',
  "referer_id" int DEFAULT NULL,
  "promotion_id" int DEFAULT NULL,
  "promotionmultiplier" int DEFAULT NULL,
  "customproperty" varchar(255) DEFAULT NULL,
  "tokenreward" bigint DEFAULT NULL,
  "optionalreceiveraddress" varchar(255) DEFAULT NULL COMMENT 'This stores the recevieraddress if specified (but it is optional)',
  "paymentmethod" enum('ADA','ETH','SOL','FIAT','APT','HBAR') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'ADA',
  "preparedpaymenttransactions_id" int DEFAULT NULL,
  "outgoingtxhash" varchar(255) DEFAULT NULL,
  "refererstring" varchar(255) DEFAULT NULL,
  "refundreceiveraddress" varchar(255) DEFAULT NULL,
  "lovelaceamountmustbeexact" tinyint(1) NOT NULL DEFAULT '1',
  "stakevkey" longtext,
  "stakeskey" longtext,
  "addresstype" enum('base','enterprise') NOT NULL DEFAULT 'enterprise',
  "coin" enum('ADA','SOL','APT','HBAR') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'ADA',
  "seedphrase" text,
  "freemint" tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY ("id") USING BTREE,
  UNIQUE KEY "nftaddresses" ("address") USING BTREE,
  UNIQUE KEY "reservationtoken" ("reservationtoken"),
  UNIQUE KEY "nftaddresses2" ("address","nftproject_id"),
  KEY "nftaddresses_nftprojects" ("nftproject_id") USING BTREE,
  KEY "nftaddressesstate" ("state","nftproject_id") USING BTREE,
  KEY "created" ("created"),
  KEY "lastcheckforutxo" ("lastcheckforutxo"),
  KEY "nftaddressstate2" ("state","serverid"),
  KEY "nftaddresses_referer" ("referer_id"),
  KEY "nftaddresses_promotion" ("promotion_id"),
  KEY "nftaddresses_preparedpaymenttransactions" ("preparedpaymenttransactions_id"),
  CONSTRAINT "nftaddresses_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "nftaddresses_preparedpaymenttransactions" FOREIGN KEY ("preparedpaymenttransactions_id") REFERENCES "preparedpaymenttransactions" ("id") ON DELETE SET NULL,
  CONSTRAINT "nftaddresses_promotion" FOREIGN KEY ("promotion_id") REFERENCES "promotions" ("id") ON DELETE SET NULL,
  CONSTRAINT "nftaddresses_referer" FOREIGN KEY ("referer_id") REFERENCES "referer" ("id") ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=862949 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for nftgroups
-- ----------------------------
DROP TABLE IF EXISTS `nftgroups`;
CREATE TABLE "nftgroups" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "groupname" varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "totaltokens1" int NOT NULL,
  "tokensreserved1" int NOT NULL,
  "tokenssold1" int NOT NULL,
  PRIMARY KEY ("id"),
  KEY "groups_nftprojects" ("nftproject_id"),
  CONSTRAINT "groups_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for nftprojectadaaddresses
-- ----------------------------
DROP TABLE IF EXISTS `nftprojectadaaddresses`;
CREATE TABLE "nftprojectadaaddresses" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftprojects_id" int NOT NULL,
  "address" varchar(255) DEFAULT NULL,
  "privateskey" longtext,
  "privatevkey" longtext,
  "lovelage" bigint DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "nftprojectsadaaddresses_nftprojects" ("nftprojects_id") USING BTREE,
  CONSTRAINT "nftprojectsadaaddresses_nftprojects" FOREIGN KEY ("nftprojects_id") REFERENCES "nftprojects" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for nftprojects
-- ----------------------------
DROP TABLE IF EXISTS `nftprojects`;
CREATE TABLE "nftprojects" (
  "id" int NOT NULL AUTO_INCREMENT,
  "customer_id" int NOT NULL,
  "projectname" varchar(255) NOT NULL,
  "projectlogo" varchar(255) DEFAULT NULL,
  "payoutaddress" varchar(255) DEFAULT NULL,
  "policyscript" longtext,
  "policyaddress" varchar(255) DEFAULT NULL COMMENT 'This address is the pay in address of the project',
  "policyid" varchar(255) DEFAULT NULL,
  "policyvkey" longtext,
  "policyskey" longtext,
  "policyexpire" datetime DEFAULT NULL,
  "state" enum('active','notactive','deleted','finished') NOT NULL,
  "password" varchar(255) NOT NULL,
  "tokennameprefix" varchar(20) DEFAULT '',
  "settings_id" int NOT NULL,
  "expiretime" int NOT NULL DEFAULT '20',
  "customerwallet_id" int DEFAULT NULL,
  "description" varchar(255) DEFAULT NULL,
  "maxsupply" bigint NOT NULL DEFAULT '1',
  "version" varchar(255) DEFAULT NULL,
  "minutxo" enum('twoadaall5nft','twoadaeverynft','minutxo') DEFAULT NULL,
  "metadata" longtext,
  "oldmetadatascheme" tinyint(1) DEFAULT '0',
  "lastupdate" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  "projecturl" varchar(255) DEFAULT NULL,
  "hasroyality" tinyint(1) NOT NULL DEFAULT '0',
  "hasidentity" tinyint(1) NOT NULL DEFAULT '0',
  "royalitypercent" float(12,2) NOT NULL DEFAULT '0.00',
  "royalityaddress" varchar(255) DEFAULT NULL,
  "royaltiycreated" datetime DEFAULT NULL,
  "activatepayinaddress" tinyint(1) NOT NULL DEFAULT '0',
  "created" datetime DEFAULT NULL,
  "total1" bigint NOT NULL DEFAULT '0',
  "free1" bigint NOT NULL DEFAULT '0',
  "reserved1" bigint NOT NULL DEFAULT '0',
  "sold1" bigint NOT NULL DEFAULT '0',
  "blocked1" bigint DEFAULT '0',
  "error1" bigint NOT NULL DEFAULT '0',
  "totaltokens1" bigint NOT NULL DEFAULT '0',
  "tokenssold1" bigint NOT NULL DEFAULT '0',
  "tokensreserved1" bigint NOT NULL DEFAULT '0',
  "countprices1" bigint NOT NULL DEFAULT '0',
  "lastinputonaddress" datetime DEFAULT NULL,
  "placeholdercsv" longtext,
  "checkcsv" tinyint(1) NOT NULL DEFAULT '0',
  "uid" varchar(255) NOT NULL,
  "alternativeaddress" varchar(255) DEFAULT NULL COMMENT 'This address wil be used, if the policyaddress is already used by an other project with the same policy id',
  "alternativepayskey" longtext,
  "alternativepayvkey" longtext,
  "smartcontractssettings_id" int NOT NULL DEFAULT '1',
  "lastcheckforutxo" datetime DEFAULT NULL,
  "maxcountmintandsend" int NOT NULL DEFAULT '15',
  "enablecrosssaleonpaywindow" tinyint(1) NOT NULL DEFAULT '1' COMMENT 'This field indicates, if we enable the cross sale feature on the paywindow (eg the NMKR Token)',
  "enablefiat" tinyint(1) NOT NULL DEFAULT '0',
  "isarchived" tinyint(1) NOT NULL DEFAULT '0',
  "donotarchive" tinyint(1) NOT NULL DEFAULT '0',
  "usdcwallet_id" int DEFAULT NULL,
  "multiplier" bigint NOT NULL DEFAULT '1',
  "lockslot" bigint DEFAULT NULL COMMENT 'When will the policy expire - the slot',
  "paymentgatewaysalestart" datetime DEFAULT NULL COMMENT 'When the PGW starts to be active',
  "enabledecentralpayments" tinyint(1) NOT NULL DEFAULT '0',
  "defaultpromotion_id" int DEFAULT NULL,
  "disablemanualmintingbutton" tinyint(1) NOT NULL DEFAULT '0',
  "disablerandomsales" tinyint(1) NOT NULL DEFAULT '0',
  "disablespecificsales" tinyint(1) NOT NULL DEFAULT '0',
  "nftsblocked" bigint DEFAULT NULL,
  "twitterhandle" varchar(255) DEFAULT NULL,
  "addrefereramounttopaymenttransactions" double(12,2) DEFAULT NULL,
  "projecttype" enum('nft-project','marketplace-whitelabel') NOT NULL DEFAULT 'nft-project',
  "marketplacewhitelabelfee" float(12,2) DEFAULT NULL,
  "nmkraccountoptions" enum('none','accountnecessary','accountandkycnecessary') NOT NULL DEFAULT 'none',
  "donotdisablepayinaddressautomatically" tinyint(1) NOT NULL DEFAULT '0',
  "crossmintcollectionid" varchar(255) DEFAULT NULL,
  "cip68" tinyint(1) NOT NULL DEFAULT '0',
  "cip68referenceaddress" varchar(255) DEFAULT NULL,
  "cip68smartcontract_id" int DEFAULT NULL,
  "mintandsendminutxo" enum('twoadaeverynft','minutxo') NOT NULL DEFAULT 'minutxo',
  "discordurl" varchar(255) DEFAULT NULL,
  "checkfiat" int NOT NULL DEFAULT '0',
  "referenceaddress" varchar(255) DEFAULT NULL,
  "referencevkey" longtext,
  "referenceskey" longtext,
  "twitterurl" varchar(255) DEFAULT NULL,
  "usefrankenprotection" tinyint(1) NOT NULL DEFAULT '0',
  "storage" enum('ipfs','iagon') NOT NULL DEFAULT 'ipfs',
  "usedstorage" bigint NOT NULL DEFAULT '0',
  "enablesolana" tinyint(1) NOT NULL DEFAULT '0' COMMENT 'obsolete',
  "enablecardano" tinyint(1) NOT NULL DEFAULT '1' COMMENT 'obsolete',
  "solanaseedphrase" text,
  "solanapublickey" varchar(255) DEFAULT NULL,
  "solanasymbol" varchar(255) DEFAULT NULL,
  "solanacustomerwallet_id" int DEFAULT NULL,
  "cip68extrafield" longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci,
  "solanacollectioncreated" datetime DEFAULT NULL,
  "solanacollectiontransaction" varchar(255) DEFAULT NULL,
  "integratesolanacollectionaddressinmetadata" tinyint(1) DEFAULT NULL,
  "integratecardanopolicyIdinmetadata" tinyint(1) DEFAULT NULL,
  "sellerFeeBasisPoints" int DEFAULT NULL,
  "solanacollectionimage" varchar(255) DEFAULT NULL,
  "solanacollectionfamily" varchar(255) DEFAULT NULL,
  "publishmintto3rdpartywebsites" tinyint(1) NOT NULL DEFAULT '0',
  "solanacollectionimagemimetype" varchar(255) DEFAULT NULL,
  "metadatatemplatename" varchar(255) DEFAULT NULL,
  "enabledcoins" varchar(255) DEFAULT 'ADA' COMMENT 'New field for all Blockchains as List of the Coins (eg: SOL APT ADA)',
  "aptoscustomerwallet_id" int DEFAULT NULL,
  "aptosaddress" varchar(255) DEFAULT NULL,
  "aptosseedphrase" text,
  "aptospublickey" varchar(255) DEFAULT NULL,
  "aptoscollectioncreated" datetime DEFAULT NULL,
  "aptoscollectiontransaction" varchar(255) DEFAULT NULL,
  "aptoscollectionimagemimetype" varchar(255) DEFAULT NULL,
  "aptoscollectionimage" varchar(255) DEFAULT NULL,
  "aptoscollectionname" varchar(255) DEFAULT NULL,
  "bitcoincustomerwallet_id" int DEFAULT NULL,
  "bitcoinaddress" varchar(255) DEFAULT NULL,
  "bitcoinseedphrase" varchar(255) DEFAULT NULL,
  "bitcoinpublickey" varchar(255) DEFAULT NULL,
  "bitcoinprivatekey" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  UNIQUE KEY "uid" ("uid"),
  KEY "nftprojects_customers" ("customer_id") USING BTREE,
  KEY "nftprojects_settings" ("settings_id") USING BTREE,
  KEY "nftprojects_customerwallets" ("customerwallet_id") USING BTREE,
  KEY "customerstate" ("customer_id","state"),
  KEY "policyid" ("policyid"),
  KEY "nftprojects_smartcontractssettings" ("smartcontractssettings_id"),
  KEY "nftprojects_usdcwallet" ("usdcwallet_id"),
  KEY "nftprojects_promotions" ("defaultpromotion_id"),
  KEY "nftprojects_smartcontract" ("cip68smartcontract_id"),
  KEY "nftprojects_customerwallets_solana" ("solanacustomerwallet_id"),
  KEY "nftprojects_solanacollectiontransaction" ("solanacollectiontransaction"),
  KEY "nftproject_customerwallets_aptos" ("aptoscustomerwallet_id"),
  KEY "nftprojects_customerwallets_bitcoin" ("bitcoincustomerwallet_id"),
  CONSTRAINT "nftproject_customerwallets_aptos" FOREIGN KEY ("aptoscustomerwallet_id") REFERENCES "customerwallets" ("id") ON DELETE SET NULL,
  CONSTRAINT "nftprojects_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "nftprojects_customerwallets" FOREIGN KEY ("customerwallet_id") REFERENCES "customerwallets" ("id") ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT "nftprojects_customerwallets_bitcoin" FOREIGN KEY ("bitcoincustomerwallet_id") REFERENCES "customerwallets" ("id") ON DELETE SET NULL,
  CONSTRAINT "nftprojects_customerwallets_solana" FOREIGN KEY ("solanacustomerwallet_id") REFERENCES "customerwallets" ("id") ON DELETE SET NULL,
  CONSTRAINT "nftprojects_promotions" FOREIGN KEY ("defaultpromotion_id") REFERENCES "promotions" ("id") ON DELETE SET NULL,
  CONSTRAINT "nftprojects_settings" FOREIGN KEY ("settings_id") REFERENCES "settings" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "nftprojects_smartcontract" FOREIGN KEY ("cip68smartcontract_id") REFERENCES "smartcontracts" ("id") ON DELETE SET NULL,
  CONSTRAINT "nftprojects_smartcontractssettings" FOREIGN KEY ("smartcontractssettings_id") REFERENCES "smartcontractsmarketplacesettings" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "nftprojects_usdcwallet" FOREIGN KEY ("usdcwallet_id") REFERENCES "customerwallets" ("id") ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=50986 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for nftprojectsadditionalpayouts
-- ----------------------------
DROP TABLE IF EXISTS `nftprojectsadditionalpayouts`;
CREATE TABLE "nftprojectsadditionalpayouts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "wallet_id" int NOT NULL,
  "valuepercent" double(12,2) DEFAULT NULL,
  "valuetotal" bigint DEFAULT NULL,
  "custompropertycondition" varchar(255) DEFAULT NULL,
  "coin" enum('ADA','SOL','APT','ETH','BTC') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'ADA',
  PRIMARY KEY ("id"),
  KEY "nftprojectsadditionalpayouts_nftprojects" ("nftproject_id"),
  KEY "nftprojectsadditionalpayouts_wallets" ("wallet_id"),
  CONSTRAINT "nftprojectsadditionalpayouts_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "nftprojectsadditionalpayouts_wallets" FOREIGN KEY ("wallet_id") REFERENCES "customerwallets" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=3657 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for nftprojectsaleconditions
-- ----------------------------
DROP TABLE IF EXISTS `nftprojectsaleconditions`;
CREATE TABLE "nftprojectsaleconditions" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "condition" enum('walletcontainspolicyid','walletdoesnotcontainpolicyid','walletdoescontainmaxpolicyid','walletcontainsminpolicyid','walletmustcontainminofpolicyid','whitlistedaddresses','stakeonpool','blacklistedaddresses','countedwhitelistedaddresses','onlyonesale') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "policyid" varchar(255) NOT NULL,
  "maxvalue" bigint DEFAULT NULL,
  "state" enum('active','notactive') CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "description" varchar(255) DEFAULT NULL,
  "policyprojectname" varchar(255) DEFAULT NULL,
  "policyid2" varchar(255) DEFAULT NULL,
  "policyid3" varchar(255) DEFAULT NULL,
  "policyid4" varchar(255) DEFAULT NULL,
  "policyid5" varchar(255) DEFAULT NULL,
  "whitlistaddresses" longtext,
  "onlyonesaleperwhitlistaddress" tinyint(1) NOT NULL DEFAULT '0',
  "usedwhitelistaddresses" longtext,
  "blacklistedaddresses" longtext,
  "operator" enum('AND','OR') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'AND',
  "policyid6" varchar(255) DEFAULT NULL,
  "policyid7" varchar(255) DEFAULT NULL,
  "policyid8" varchar(255) DEFAULT NULL,
  "policyid9" varchar(255) DEFAULT NULL,
  "policyid10" varchar(255) DEFAULT NULL,
  "policyid11" varchar(255) DEFAULT NULL,
  "policyid12" varchar(255) DEFAULT NULL,
  "policyid13" varchar(255) DEFAULT NULL,
  "policyid14" varchar(255) DEFAULT NULL,
  "policyid15" varchar(255) DEFAULT NULL,
  "blockchain" enum('Cardano','Solana','Aptos') NOT NULL DEFAULT 'Cardano',
  PRIMARY KEY ("id"),
  KEY "nftprojectsaleconditions_nftprojects" ("nftproject_id"),
  CONSTRAINT "nftprojectsaleconditions_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=2983 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for nftprojectsendpremintedtokens
-- ----------------------------
DROP TABLE IF EXISTS `nftprojectsendpremintedtokens`;
CREATE TABLE "nftprojectsendpremintedtokens" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "blockchain_id" int NOT NULL,
  "policyid_or_collection" varchar(255) NOT NULL,
  "tokenname" varchar(255) NOT NULL,
  "countokenstosend" bigint NOT NULL,
  "state" enum('active','notactive') NOT NULL,
  "sendwithmintandsend" tinyint(1) NOT NULL DEFAULT '0',
  "sendwithapiaddresses" tinyint(1) NOT NULL DEFAULT '0',
  "sendwithmultisigtransactions" tinyint(1) NOT NULL DEFAULT '0',
  "sendwithpayinaddresses" tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY ("id"),
  KEY "nftprojectsendpremintedtokens_nftprojects" ("nftproject_id"),
  KEY "nftprojectsendpremintedtokens_blockchains" ("blockchain_id"),
  CONSTRAINT "nftprojectsendpremintedtokens_blockchains" FOREIGN KEY ("blockchain_id") REFERENCES "activeblockchains" ("id") ON DELETE RESTRICT,
  CONSTRAINT "nftprojectsendpremintedtokens_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for nftreservations
-- ----------------------------
DROP TABLE IF EXISTS `nftreservations`;
CREATE TABLE "nftreservations" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nft_id" int NOT NULL,
  "reservationtoken" varchar(255) NOT NULL,
  "reservationdate" datetime NOT NULL,
  "reservationtime" int NOT NULL DEFAULT '60',
  "tc" bigint NOT NULL DEFAULT '0',
  "serverid" int DEFAULT NULL,
  "mintandsendcommand" tinyint(1) NOT NULL DEFAULT '0',
  "multiplier" bigint NOT NULL DEFAULT '1' COMMENT 'not used at the moment',
  PRIMARY KEY ("id"),
  UNIQUE KEY "nftreservations1" ("reservationtoken","nft_id"),
  KEY "nftreservations_nfts" ("nft_id"),
  CONSTRAINT "nftreservations_nfts" FOREIGN KEY ("nft_id") REFERENCES "nfts" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=18909953 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for nfts
-- ----------------------------
DROP TABLE IF EXISTS `nfts`;
CREATE TABLE "nfts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "filename" varchar(255) NOT NULL,
  "ipfshash" varchar(255) NOT NULL,
  "state" enum('free','sold','reserved','deleted','error','signed','burned','blocked') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "name" varchar(255) NOT NULL COMMENT 'Tokenprefix (from Projects) + Name = Assetname',
  "displayname" varchar(255) DEFAULT NULL,
  "mainnft_id" int DEFAULT NULL COMMENT 'If not Null, it is the second (High Resolution Image of the Main Pic) - Used in the Unsig Project',
  "minted" tinyint(1) NOT NULL COMMENT 'Shows, if the NFT is already minted',
  "detaildata" longtext,
  "metadatatemplate_id" int DEFAULT NULL,
  "receiveraddress" varchar(255) DEFAULT NULL,
  "transactionid" varchar(255) DEFAULT NULL,
  "mintingfees" bigint DEFAULT NULL,
  "mintingfeespaymentaddress" varchar(255) DEFAULT NULL,
  "mintingfeestransactionid" varchar(255) DEFAULT NULL,
  "selldate" datetime DEFAULT NULL,
  "soldby" enum('normal','manual','coupon') DEFAULT NULL,
  "buildtransaction" longtext,
  "reserveduntil" datetime DEFAULT NULL,
  "testmarker" int DEFAULT NULL,
  "policyid" varchar(255) DEFAULT NULL COMMENT 'The Policy ID should be the same as in the Project - but we load it from Blockfrost to verify',
  "assetid" varchar(255) DEFAULT NULL COMMENT 'Value from Blockfrost',
  "assetname" varchar(255) DEFAULT NULL COMMENT 'Value from Blockfrost',
  "fingerprint" varchar(255) DEFAULT NULL COMMENT 'Value from Blockfrost',
  "initialminttxhash" varchar(255) DEFAULT NULL COMMENT 'Value from Blockfrost',
  "title" varchar(255) DEFAULT NULL COMMENT 'Value from Blockfrost',
  "series" varchar(255) DEFAULT NULL COMMENT 'Value from Blockfrost',
  "markedaserror" datetime DEFAULT NULL,
  "instockpremintedaddress_id" int DEFAULT NULL COMMENT 'When the NFT is already minted and in Stock - here is the ID of the Address where it is',
  "checkpolicyid" tinyint(1) DEFAULT NULL COMMENT 'When true - The program searches for the policyid/fingerprint on blockforst',
  "mimetype" varchar(255) DEFAULT 'image/png',
  "soldcount" bigint NOT NULL DEFAULT '0',
  "reservedcount" bigint NOT NULL DEFAULT '0',
  "errorcount" bigint NOT NULL DEFAULT '0',
  "burncount" bigint NOT NULL DEFAULT '0',
  "metadataoverride" longtext,
  "lastpolicycheck" datetime DEFAULT NULL,
  "uploadedtonftstorage" tinyint(1) NOT NULL DEFAULT '0',
  "isroyaltytoken" tinyint(1) NOT NULL DEFAULT '0',
  "filesize" bigint NOT NULL DEFAULT '0',
  "created" datetime DEFAULT NULL,
  "price" bigint DEFAULT NULL,
  "nftgroup_id" int DEFAULT NULL,
  "uid" varchar(40) NOT NULL,
  "reservationtoken" varchar(255) DEFAULT NULL,
  "multiplier" bigint NOT NULL DEFAULT '1',
  "uploadsource" varchar(255) DEFAULT NULL,
  "cipversion" enum('unknown','cip20','cip68','cip25') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT 'unknown',
  "metadataoverridecip68" longtext,
  "iagonid" varchar(255) DEFAULT NULL,
  "iagonuploadresult" text,
  "solanacollectionnft" varchar(255) DEFAULT NULL,
  "verifiedcollectionsolana" enum('mustbeadded','success','error','nocollection') DEFAULT NULL,
  "verifiedcollectionsignature" varchar(255) DEFAULT NULL,
  "mintedonblockchain" enum('Solana','Cardano','Aptos','Bitcoin') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT 'Cardano',
  "solanatokenhash" varchar(255) DEFAULT NULL,
  "pricesolana" bigint DEFAULT NULL,
  "priceaptos" bigint DEFAULT NULL,
  "pricemidnight" bigint DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  UNIQUE KEY "uid" ("uid") USING BTREE,
  KEY "nfts_nfts" ("mainnft_id") USING BTREE,
  KEY "nftprojectstate" ("nftproject_id","state") USING BTREE,
  KEY "nftprojectstate2" ("nftproject_id","state","mainnft_id") USING BTREE,
  KEY "ntfs_metadatatemplates" ("metadatatemplate_id") USING BTREE,
  KEY "nfts_premintedaddresses" ("instockpremintedaddress_id") USING BTREE,
  KEY "assetid" ("assetid"),
  KEY "fingerprint" ("state","fingerprint","mainnft_id"),
  KEY "name" ("name") USING BTREE,
  KEY "nftprojectid" ("nftproject_id"),
  KEY "nfts_nftgroups" ("nftgroup_id"),
  KEY "checkpolicyid" ("checkpolicyid"),
  KEY "fingerprint_soldcount" ("fingerprint","soldcount"),
  KEY "reservationtoken" ("reservationtoken"),
  KEY "ipfs" ("ipfshash") USING BTREE,
  KEY "nfts_verifiedcollectionsolana" ("verifiedcollectionsolana"),
  CONSTRAINT "nfts_nftgroups" FOREIGN KEY ("nftgroup_id") REFERENCES "nftgroups" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "nfts_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "nfts_nfts" FOREIGN KEY ("mainnft_id") REFERENCES "nfts" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "nfts_premintedaddresses" FOREIGN KEY ("instockpremintedaddress_id") REFERENCES "premintednftsaddresses" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=17394017 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for nfts_archive
-- ----------------------------
DROP TABLE IF EXISTS `nfts_archive`;
CREATE TABLE "nfts_archive" (
  "id" int NOT NULL,
  "nftproject_id" int NOT NULL,
  "filename" varchar(255) NOT NULL,
  "ipfshash" varchar(255) NOT NULL,
  "state" enum('free','sold','reserved','deleted','error','signed','burned') NOT NULL,
  "name" varchar(255) NOT NULL COMMENT 'Tokenprefix (from Projects) + Name = Assetname',
  "displayname" varchar(255) DEFAULT NULL,
  "mainnft_id" int DEFAULT NULL COMMENT 'If not Null, it is the second (High Resolution Image of the Main Pic) - Used in the Unsig Project',
  "minted" tinyint(1) NOT NULL COMMENT 'Shows, if the NFT is already minted',
  "detaildata" longtext,
  "metadatatemplate_id" int DEFAULT NULL,
  "receiveraddress" varchar(255) DEFAULT NULL,
  "transactionid" varchar(255) DEFAULT NULL,
  "mintingfees" bigint DEFAULT NULL,
  "mintingfeespaymentaddress" varchar(255) DEFAULT NULL,
  "mintingfeestransactionid" varchar(255) DEFAULT NULL,
  "selldate" datetime DEFAULT NULL,
  "soldby" enum('normal','manual','coupon') DEFAULT NULL,
  "buildtransaction" longtext,
  "reserveduntil" datetime DEFAULT NULL,
  "testmarker" int DEFAULT NULL,
  "policyid" varchar(255) DEFAULT NULL COMMENT 'The Policy ID should be the same as in the Project - but we load it from Blockfrost to verify',
  "assetid" varchar(255) DEFAULT NULL COMMENT 'Value from Blockfrost',
  "assetname" varchar(255) DEFAULT NULL COMMENT 'Value from Blockfrost',
  "fingerprint" varchar(255) DEFAULT NULL COMMENT 'Value from Blockfrost',
  "initialminttxhash" varchar(255) DEFAULT NULL COMMENT 'Value from Blockfrost',
  "title" varchar(255) DEFAULT NULL COMMENT 'Value from Blockfrost',
  "series" varchar(255) DEFAULT NULL COMMENT 'Value from Blockfrost',
  "markedaserror" datetime DEFAULT NULL,
  "instockpremintedaddress_id" int DEFAULT NULL COMMENT 'When the NFT is already minted and in Stock - here is the ID of the Address where it is',
  "checkpolicyid" tinyint(1) DEFAULT NULL COMMENT 'When true - The program searches for the policyid/fingerprint on blockforst',
  "mimetype" varchar(255) DEFAULT 'image/png',
  "soldcount" bigint NOT NULL DEFAULT '0',
  "reservedcount" bigint NOT NULL DEFAULT '0',
  "errorcount" bigint NOT NULL DEFAULT '0',
  "burncount" bigint NOT NULL DEFAULT '0',
  "metadataoverride" longtext,
  "lastpolicycheck" datetime DEFAULT NULL,
  "uploadedtonftstorage" tinyint(1) NOT NULL DEFAULT '0',
  "isroyaltytoken" tinyint(1) NOT NULL DEFAULT '0',
  "filesize" bigint DEFAULT NULL,
  "created" datetime DEFAULT NULL,
  "price" bigint DEFAULT NULL,
  "nftgroup_id" int DEFAULT NULL,
  "uid" varchar(40) NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  UNIQUE KEY "uid" ("uid") USING BTREE,
  KEY "nfts_nfts" ("mainnft_id") USING BTREE,
  KEY "nftprojectstate" ("nftproject_id","state") USING BTREE,
  KEY "nftprojectstate2" ("nftproject_id","state","mainnft_id") USING BTREE,
  KEY "ntfs_metadatatemplates" ("metadatatemplate_id") USING BTREE,
  KEY "nfts_premintedaddresses" ("instockpremintedaddress_id") USING BTREE,
  KEY "ipfs" ("ipfshash","nftproject_id","instockpremintedaddress_id") USING BTREE,
  KEY "uploaded" ("uploadedtonftstorage"),
  KEY "assetid" ("assetid"),
  KEY "fingerprint" ("state","fingerprint","mainnft_id"),
  KEY "name" ("name") USING BTREE,
  KEY "nftprojectid" ("nftproject_id"),
  KEY "nfts_nftgroups" ("nftgroup_id"),
  CONSTRAINT "nfts_archive_ibfk_1" FOREIGN KEY ("nftgroup_id") REFERENCES "nftgroups" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "nfts_archive_ibfk_2" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "nfts_archive_ibfk_3" FOREIGN KEY ("mainnft_id") REFERENCES "nfts" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "nfts_archive_ibfk_5" FOREIGN KEY ("metadatatemplate_id") REFERENCES "metadatatemplate" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for nfttonftaddresses
-- ----------------------------
DROP TABLE IF EXISTS `nfttonftaddresses`;
CREATE TABLE "nfttonftaddresses" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nft_id" int NOT NULL,
  "nftaddresses_id" int NOT NULL,
  "tokencount" bigint NOT NULL DEFAULT '1',
  PRIMARY KEY ("id") USING BTREE,
  UNIQUE KEY "nfttoaddresses" ("nft_id","nftaddresses_id") USING BTREE,
  KEY "nfttonftaddresses_nftaddresses" ("nftaddresses_id") USING BTREE,
  KEY "nftid" ("nft_id") USING BTREE,
  CONSTRAINT "afttoaftaddresses_nfts" FOREIGN KEY ("nft_id") REFERENCES "nfts" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "nfttonftaddresses_nftaddresses" FOREIGN KEY ("nftaddresses_id") REFERENCES "nftaddresses" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=2815510 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for nfttonftaddresseshistory
-- ----------------------------
DROP TABLE IF EXISTS `nfttonftaddresseshistory`;
CREATE TABLE "nfttonftaddresseshistory" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nft_id" int NOT NULL,
  "nftaddresses_id" int NOT NULL,
  "created" datetime NOT NULL,
  "tokencount" bigint NOT NULL DEFAULT '1',
  PRIMARY KEY ("id") USING BTREE,
  KEY "nfttonftaddresses_nftaddresses" ("nftaddresses_id") USING BTREE,
  KEY "nfttoaddresses" ("nft_id","nftaddresses_id") USING BTREE,
  KEY "nftid" ("nft_id") USING BTREE,
  CONSTRAINT "afttoaftaddresseshistory_nfts" FOREIGN KEY ("nft_id") REFERENCES "nfts" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "nfttonftaddresseshistory_nftaddresses" FOREIGN KEY ("nftaddresses_id") REFERENCES "nftaddresses" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=1291474 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for notificationqueue
-- ----------------------------
DROP TABLE IF EXISTS `notificationqueue`;
CREATE TABLE "notificationqueue" (
  "id" int NOT NULL AUTO_INCREMENT,
  "state" enum('active','processing','successful','error') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "notificationtype" enum('email','webhook') NOT NULL,
  "notificationendpoint" varchar(255) NOT NULL,
  "payload" text NOT NULL,
  "server_id" int DEFAULT NULL,
  "created" datetime NOT NULL,
  "processed" datetime DEFAULT NULL,
  "result" varchar(255) DEFAULT NULL,
  "counterrors" int NOT NULL DEFAULT '0',
  "lasterror" datetime DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "notificationstate" ("state")
) ENGINE=InnoDB AUTO_INCREMENT=78 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for notifications
-- ----------------------------
DROP TABLE IF EXISTS `notifications`;
CREATE TABLE "notifications" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "notificationtype" enum('webhook','email') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "address" varchar(255) NOT NULL,
  "state" enum('active','notactive','deleted') NOT NULL,
  "secret" varchar(255) NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "notifications_nftprojects" ("nftproject_id") USING BTREE,
  CONSTRAINT "notifications_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=250 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for onlinenotifications
-- ----------------------------
DROP TABLE IF EXISTS `onlinenotifications`;
CREATE TABLE "onlinenotifications" (
  "id" int NOT NULL AUTO_INCREMENT,
  "customer_id" int NOT NULL,
  "notificationmessage" longtext NOT NULL,
  "created" datetime NOT NULL,
  "state" enum('new','hasread') NOT NULL,
  "color" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id"),
  KEY "onlinenotifications_customers" ("customer_id"),
  CONSTRAINT "onlinenotifications_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=49358 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for paybuttoncode
-- ----------------------------
DROP TABLE IF EXISTS `paybuttoncode`;
CREATE TABLE "paybuttoncode" (
  "id" int NOT NULL AUTO_INCREMENT,
  "description" varchar(255) NOT NULL,
  "code" longtext NOT NULL,
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for payoutrequests
-- ----------------------------
DROP TABLE IF EXISTS `payoutrequests`;
CREATE TABLE "payoutrequests" (
  "id" int NOT NULL AUTO_INCREMENT,
  "customer_id" int NOT NULL,
  "wallet_id" int NOT NULL,
  "ada" bigint NOT NULL,
  "created" datetime NOT NULL,
  "confirmationcode" varchar(255) NOT NULL,
  "confirmationexpire" datetime NOT NULL,
  "confirmationipaddress" varchar(255) DEFAULT NULL,
  "state" enum('active','expired','executed','execute','error') NOT NULL,
  "executiontime" datetime DEFAULT NULL,
  "payoutinitiator" enum('website','api') NOT NULL,
  "confirmationtime" datetime DEFAULT NULL,
  "transactionid" varchar(255) DEFAULT NULL,
  "logfile" longtext,
  PRIMARY KEY ("id") USING BTREE,
  KEY "payoutrequests_customers" ("customer_id") USING BTREE,
  KEY "payoutrequests_customerwallets" ("wallet_id") USING BTREE,
  CONSTRAINT "payoutrequests_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "payoutrequests_customerwallets" FOREIGN KEY ("wallet_id") REFERENCES "customerwallets" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=3590 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for plugins
-- ----------------------------
DROP TABLE IF EXISTS `plugins`;
CREATE TABLE "plugins" (
  "id" int NOT NULL AUTO_INCREMENT,
  "image" varchar(255) NOT NULL,
  "header" varchar(255) NOT NULL,
  "subtitle" varchar(255) DEFAULT NULL,
  "description" longtext,
  "buttontext" varchar(255) DEFAULT NULL,
  "buttonlink" varchar(255) DEFAULT NULL,
  "state" enum('active','notactive') NOT NULL,
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for premintednftsaddresses
-- ----------------------------
DROP TABLE IF EXISTS `premintednftsaddresses`;
CREATE TABLE "premintednftsaddresses" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int DEFAULT NULL,
  "address" varchar(255) NOT NULL,
  "privateskey" longtext NOT NULL,
  "privatevkey" longtext NOT NULL,
  "salt" varchar(255) NOT NULL,
  "lovelace" bigint NOT NULL,
  "lastcheckforutxo" datetime NOT NULL,
  "state" enum('free','reserved','inuse','send','error') NOT NULL,
  "created" datetime NOT NULL,
  "expires" datetime DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "premintednftsaddresses_nftprojects" ("nftproject_id") USING BTREE,
  CONSTRAINT "premintednftsaddresses_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=3254 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for premintedpromotokenaddresses
-- ----------------------------
DROP TABLE IF EXISTS `premintedpromotokenaddresses`;
CREATE TABLE "premintedpromotokenaddresses" (
  "id" int NOT NULL AUTO_INCREMENT,
  "blockchain_id" int NOT NULL,
  "tokenname" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "policyid_or_collection" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "address" varchar(255) NOT NULL,
  "seedphrase" text,
  "privatekey" text,
  "publickey" text,
  "salt" varchar(255) NOT NULL,
  "totaltokens" bigint NOT NULL,
  "state" enum('active','blocked','empty','disabled') CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "lastcheck" datetime NOT NULL,
  "blockdate" datetime DEFAULT NULL,
  "reservationtoken" varchar(255) DEFAULT NULL,
  "lasttxhash" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id"),
  KEY "promotokenaddresses_blockchains" ("blockchain_id"),
  CONSTRAINT "promotokenaddresses_blockchains" FOREIGN KEY ("blockchain_id") REFERENCES "activeblockchains" ("id") ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for preparedpaymenttransactions
-- ----------------------------
DROP TABLE IF EXISTS `preparedpaymenttransactions`;
CREATE TABLE "preparedpaymenttransactions" (
  "id" int NOT NULL AUTO_INCREMENT,
  "transactionuid" varchar(255) NOT NULL,
  "transactiontype" enum('paymentgateway_nft_specific','paymentgateway_nft_random','smartcontract_directsale','smartcontract_auction','legacy_auction','legacy_directsale','decentral_mintandsend_random','decentral_mintandsend_specific','paymentgateway_mintandsend_random','paymentgateway_mintandsend_specific','decentral_mintandsale_random','decentral_mintandsale_specific','nmkr_pay_random','nmkr_pay_specific','smartcontract_directsale_offer','paymentgateway_buyout_smartcontract') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "nftproject_id" int NOT NULL,
  "lovelace" bigint DEFAULT NULL,
  "created" datetime NOT NULL,
  "transaction_id" int DEFAULT NULL,
  "reservation_id" int DEFAULT NULL,
  "state" enum('active','expired','finished','prepared','error','canceled','rejected') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  "countnft" bigint DEFAULT NULL,
  "customeripaddress" varchar(255) DEFAULT NULL,
  "nftaddresses_id" int DEFAULT NULL,
  "smartcontracts_id" int DEFAULT NULL,
  "policyid" varchar(255) DEFAULT NULL,
  "tokencount" bigint DEFAULT NULL,
  "tokenname" varchar(255) DEFAULT NULL,
  "auctionminprice" bigint DEFAULT NULL,
  "auctionduration" int DEFAULT NULL,
  "smartcontractstate" enum('prepared','waitingforbid','sold','canceled','readytosignbyseller','readytosignbybuyer','auctionexpired','waitingforsale','waitingforlocknft','submitted','confirmed','readytosignbysellercancel','waitingforlockada','readytosignbybuyercancel') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  "paymentgatewaystate" enum('prepared','sold','canceled','readytosignbybuyer','signedbybuyer','submitted') DEFAULT NULL,
  "cachedresultgetpaymentaddress" text,
  "sellerpkh" varchar(255) DEFAULT NULL,
  "selleraddress" varchar(255) DEFAULT NULL,
  "estimatedfees" bigint DEFAULT NULL,
  "smartcontractsmarketplace_id" int NOT NULL,
  "buyerpkh" varchar(255) DEFAULT NULL,
  "buyeraddresses" text,
  "cbor" text,
  "signedcbor" text,
  "changeaddress" varchar(255) DEFAULT NULL,
  "buyeraddress" varchar(255) DEFAULT NULL,
  "selleraddresses" text,
  "reservationtoken" varchar(255) DEFAULT NULL,
  "command" text,
  "logfile" longtext CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci,
  "expires" datetime DEFAULT NULL,
  "fee" bigint DEFAULT NULL,
  "legacyauctions_id" int DEFAULT NULL,
  "lockamount" bigint DEFAULT NULL,
  "legacydirectsales_id" int DEFAULT NULL,
  "mintandsend_id" int DEFAULT NULL,
  "stakerewards" bigint DEFAULT NULL,
  "discount" bigint DEFAULT NULL,
  "rejectparameter" varchar(255) DEFAULT NULL,
  "rejectreason" varchar(255) DEFAULT NULL,
  "txhash" varchar(255) DEFAULT NULL,
  "submitteddate" datetime DEFAULT NULL,
  "confirmeddate" datetime DEFAULT NULL,
  "referer" varchar(255) DEFAULT NULL,
  "createroyaltytokenaddress" varchar(255) DEFAULT NULL,
  "createroyaltytokenpercentage" float(12,2) DEFAULT NULL,
  "promotion_id" int DEFAULT NULL,
  "promotionmultiplier" int DEFAULT NULL,
  "referencedprepearedtransaction_id" int DEFAULT NULL,
  "txinforalreadylockedtransactions" varchar(255) DEFAULT NULL,
  "tokenrewards" bigint DEFAULT NULL,
  "overridemarketplacefee" float(12,2) DEFAULT NULL,
  "overridemarketplaceaddress" varchar(255) DEFAULT NULL,
  "buyoutaddresses_id" int DEFAULT NULL,
  "optionalreceiveraddress" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id"),
  UNIQUE KEY "preparedpaymenttransactionsuid" ("transactionuid"),
  KEY "preparedpaymenttransactions_transactions" ("transaction_id"),
  KEY "preparedpaymenttransactions_reservations" ("reservation_id"),
  KEY "preparedpaymenttransactions_projects" ("nftproject_id"),
  KEY "preparedpaymenttransactions_nftaddresses" ("nftaddresses_id"),
  KEY "preparedpaymenttransactions_smartcontracts" ("smartcontracts_id"),
  KEY "preparedpaymenttransactions_legacyauctions" ("legacyauctions_id"),
  KEY "preparedpaymenttransactions_legacydirectsales" ("legacydirectsales_id"),
  KEY "preparedpaymenttransactions_mintandsend" ("mintandsend_id"),
  KEY "preparedpaymenttransactions_promotions" ("promotion_id"),
  KEY "preparedpaymenttransactions_preparedpaymenttransactions" ("referencedprepearedtransaction_id"),
  KEY "preparedpaymenttransactions_buyoutaddresses" ("buyoutaddresses_id"),
  CONSTRAINT "preparedpaymenttransactions_buyoutaddresses" FOREIGN KEY ("buyoutaddresses_id") REFERENCES "buyoutsmartcontractaddresses" ("id") ON DELETE SET NULL,
  CONSTRAINT "preparedpaymenttransactions_legacyauctions" FOREIGN KEY ("legacyauctions_id") REFERENCES "legacyauctions" ("id") ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT "preparedpaymenttransactions_legacydirectsales" FOREIGN KEY ("legacydirectsales_id") REFERENCES "legacydirectsales" ("id") ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT "preparedpaymenttransactions_mintandsend" FOREIGN KEY ("mintandsend_id") REFERENCES "mintandsend" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "preparedpaymenttransactions_nftaddresses" FOREIGN KEY ("nftaddresses_id") REFERENCES "nftaddresses" ("id") ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT "preparedpaymenttransactions_preparedpaymenttransactions" FOREIGN KEY ("referencedprepearedtransaction_id") REFERENCES "preparedpaymenttransactions" ("id") ON DELETE SET NULL,
  CONSTRAINT "preparedpaymenttransactions_projects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "preparedpaymenttransactions_promotions" FOREIGN KEY ("promotion_id") REFERENCES "promotions" ("id") ON DELETE SET NULL,
  CONSTRAINT "preparedpaymenttransactions_reservations" FOREIGN KEY ("reservation_id") REFERENCES "nftreservations" ("id") ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT "preparedpaymenttransactions_smartcontracts" FOREIGN KEY ("smartcontracts_id") REFERENCES "smartcontracts" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "preparedpaymenttransactions_transactions" FOREIGN KEY ("transaction_id") REFERENCES "transactions" ("id") ON DELETE SET NULL ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=11254445 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for preparedpaymenttransactions_customproperties
-- ----------------------------
DROP TABLE IF EXISTS `preparedpaymenttransactions_customproperties`;
CREATE TABLE "preparedpaymenttransactions_customproperties" (
  "id" int NOT NULL AUTO_INCREMENT,
  "preparedpaymenttransactions_id" int NOT NULL,
  "key" varchar(255) NOT NULL,
  "value" varchar(255) NOT NULL,
  PRIMARY KEY ("id"),
  KEY "preparedpaymenttransactions_customproperties" ("preparedpaymenttransactions_id"),
  CONSTRAINT "preparedpaymenttransactions_customproperties" FOREIGN KEY ("preparedpaymenttransactions_id") REFERENCES "preparedpaymenttransactions" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=9064 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for preparedpaymenttransactions_nfts
-- ----------------------------
DROP TABLE IF EXISTS `preparedpaymenttransactions_nfts`;
CREATE TABLE "preparedpaymenttransactions_nfts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "preparedpaymenttransactions_id" int NOT NULL,
  "nft_id" int DEFAULT NULL,
  "count" bigint NOT NULL,
  "policyid" varchar(255) DEFAULT NULL,
  "tokenname" varchar(255) DEFAULT NULL,
  "tokennamehex" varchar(255) DEFAULT NULL,
  "lovelace" bigint DEFAULT NULL,
  "nftuid" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id"),
  KEY "preparedpaymenttransactions_preparedpaymenttransactionsnfts" ("preparedpaymenttransactions_id"),
  KEY "preparedpaymenttransactionsnfts_nfts" ("nft_id"),
  CONSTRAINT "preparedpaymenttransactions_preparedpaymenttransactionsnfts" FOREIGN KEY ("preparedpaymenttransactions_id") REFERENCES "preparedpaymenttransactions" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "preparedpaymenttransactionsnfts_nfts" FOREIGN KEY ("nft_id") REFERENCES "nfts" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=210453 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for preparedpaymenttransactions_notifications
-- ----------------------------
DROP TABLE IF EXISTS `preparedpaymenttransactions_notifications`;
CREATE TABLE "preparedpaymenttransactions_notifications" (
  "id" int NOT NULL AUTO_INCREMENT,
  "preparedpaymenttransactions_id" int NOT NULL,
  "notificationtype" enum('webhook','email') NOT NULL,
  "notificationendpoint" varchar(255) NOT NULL,
  "secret" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '',
  PRIMARY KEY ("id"),
  KEY "notifications_preparedpaymenttransactions" ("preparedpaymenttransactions_id"),
  CONSTRAINT "notifications_preparedpaymenttransactions" FOREIGN KEY ("preparedpaymenttransactions_id") REFERENCES "preparedpaymenttransactions" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=3365 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for preparedpaymenttransactions_smartcontract_outputs
-- ----------------------------
DROP TABLE IF EXISTS `preparedpaymenttransactions_smartcontract_outputs`;
CREATE TABLE "preparedpaymenttransactions_smartcontract_outputs" (
  "id" int NOT NULL AUTO_INCREMENT,
  "preparedpaymenttransactions_id" int NOT NULL,
  "address" varchar(255) NOT NULL,
  "lovelace" bigint NOT NULL,
  "pkh" varchar(255) NOT NULL,
  "type" enum('seller','buyer','marketplace','royalties','referer','unknown','nmkr') CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'unknown',
  PRIMARY KEY ("id"),
  KEY "preparedpaymenttransactions_smartcontract_outputs" ("preparedpaymenttransactions_id"),
  CONSTRAINT "preparedpaymenttransactions_smartcontract_outputs" FOREIGN KEY ("preparedpaymenttransactions_id") REFERENCES "preparedpaymenttransactions" ("id") ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4154 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for preparedpaymenttransactions_smartcontract_outputs_assets
-- ----------------------------
DROP TABLE IF EXISTS `preparedpaymenttransactions_smartcontract_outputs_assets`;
CREATE TABLE "preparedpaymenttransactions_smartcontract_outputs_assets" (
  "id" int NOT NULL AUTO_INCREMENT,
  "preparedpaymenttransactions_smartcontract_outputs_id" int NOT NULL,
  "tokennameinhex" varchar(255) NOT NULL,
  "policyid" varchar(255) NOT NULL,
  "amount" bigint NOT NULL,
  PRIMARY KEY ("id"),
  KEY "preparedpaymenttransactions_smartcontract_outputs_assets" ("preparedpaymenttransactions_smartcontract_outputs_id"),
  CONSTRAINT "preparedpaymenttransactions_smartcontract_outputs_assets" FOREIGN KEY ("preparedpaymenttransactions_smartcontract_outputs_id") REFERENCES "preparedpaymenttransactions_smartcontract_outputs" ("id") ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for preparedpaymenttransactions_smartcontractsjsons
-- ----------------------------
DROP TABLE IF EXISTS `preparedpaymenttransactions_smartcontractsjsons`;
CREATE TABLE "preparedpaymenttransactions_smartcontractsjsons" (
  "id" int NOT NULL AUTO_INCREMENT,
  "preparedpaymenttransactions_id" int NOT NULL,
  "templatetype" varchar(255) NOT NULL,
  "json" text NOT NULL,
  "hash" varchar(255) NOT NULL,
  "address" varchar(255) DEFAULT NULL,
  "rawtx" text,
  "created" datetime DEFAULT NULL,
  "signed" datetime DEFAULT NULL,
  "submitted" datetime DEFAULT NULL,
  "signedcbr" text,
  "txid" varchar(255) DEFAULT NULL,
  "redeemer" text,
  "fee" bigint DEFAULT NULL,
  "bidamount" bigint DEFAULT NULL,
  "logfile" longtext,
  "signinguid" varchar(255) DEFAULT NULL,
  "signedandsubmitted" tinyint(1) NOT NULL DEFAULT '0',
  "confirmed" tinyint(1) NOT NULL DEFAULT '0',
  "checkforconfirmdate" datetime DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "smartcontractsjsons_preparedpaymenttransactions" ("preparedpaymenttransactions_id"),
  KEY "smartcontractjsons_txid" ("txid"),
  CONSTRAINT "smartcontractsjsons_preparedpaymenttransactions" FOREIGN KEY ("preparedpaymenttransactions_id") REFERENCES "preparedpaymenttransactions" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=72178 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for preparedpaymenttransactions_tokenprice
-- ----------------------------
DROP TABLE IF EXISTS `preparedpaymenttransactions_tokenprice`;
CREATE TABLE "preparedpaymenttransactions_tokenprice" (
  "id" int NOT NULL AUTO_INCREMENT,
  "preparedpaymenttransaction_id" int NOT NULL,
  "policyid" varchar(255) NOT NULL,
  "assetname" varchar(255) DEFAULT NULL,
  "tokencount" bigint NOT NULL,
  "totalcount" bigint NOT NULL,
  "multiplier" bigint NOT NULL DEFAULT '1',
  "decimals" bigint NOT NULL DEFAULT '0',
  "assetnamehex" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id"),
  KEY "tokenprice_preparedpaymenttransactions" ("preparedpaymenttransaction_id"),
  CONSTRAINT "tokenprice_preparedpaymenttransactions" FOREIGN KEY ("preparedpaymenttransaction_id") REFERENCES "preparedpaymenttransactions" ("id") ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1043 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for pricelist
-- ----------------------------
DROP TABLE IF EXISTS `pricelist`;
CREATE TABLE "pricelist" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "countnftortoken" bigint NOT NULL,
  "priceinlovelace" bigint NOT NULL,
  "validfrom" datetime DEFAULT NULL,
  "validto" datetime DEFAULT NULL,
  "state" enum('active','notactive') DEFAULT NULL,
  "nftgroup_id" int DEFAULT NULL,
  "priceintoken" bigint DEFAULT NULL,
  "tokenpolicyid" varchar(255) DEFAULT NULL,
  "tokenassetid" varchar(255) DEFAULT NULL,
  "currency" enum('ADA','EUR','USD','JPY','SOL','APT','BTC') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'ADA',
  "changeaddresswhenpaywithtokens" enum('seller','buyer') DEFAULT NULL,
  "promotion_id" int DEFAULT NULL,
  "promotionmultiplier" int DEFAULT NULL,
  "tokenmultiplier" bigint DEFAULT '1',
  "assetnamehex" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id"),
  KEY "pricelist_nftprojects" ("nftproject_id"),
  KEY "pricelist_nftgroups" ("nftgroup_id"),
  KEY "pricelist_promotions" ("promotion_id"),
  CONSTRAINT "pricelist_nftgroups" FOREIGN KEY ("nftgroup_id") REFERENCES "nftgroups" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "pricelist_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "pricelist_promotions" FOREIGN KEY ("promotion_id") REFERENCES "promotions" ("id") ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=45337 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for pricelistdiscounts
-- ----------------------------
DROP TABLE IF EXISTS `pricelistdiscounts`;
CREATE TABLE "pricelistdiscounts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "condition" enum('walletcontainsminofpolicyid','whitlistedaddresses','stakeonpool','referercode','couponcode') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "policyid" varchar(255) NOT NULL,
  "minvalue" bigint DEFAULT NULL,
  "state" enum('active','notactive') NOT NULL,
  "description" varchar(255) DEFAULT NULL,
  "policyprojectname" varchar(255) DEFAULT NULL,
  "policyid2" varchar(255) DEFAULT NULL,
  "policyid3" varchar(255) DEFAULT NULL,
  "policyid4" varchar(255) DEFAULT NULL,
  "policyid5" varchar(255) DEFAULT NULL,
  "whitlistaddresses" longtext,
  "sendbackdiscount" float(12,2) NOT NULL DEFAULT '0.00',
  "operator" enum('AND','OR') NOT NULL DEFAULT 'OR',
  "minvalue2" bigint DEFAULT NULL,
  "minvalue3" bigint DEFAULT NULL,
  "minvalue4" bigint DEFAULT NULL,
  "minvalue5" bigint DEFAULT NULL,
  "referercode" varchar(255) DEFAULT NULL,
  "couponcode" varchar(255) DEFAULT NULL,
  "blockchain" enum('Cardano','Solana','Aptos') NOT NULL DEFAULT 'Cardano',
  PRIMARY KEY ("id"),
  KEY "pricelistdiscounts_nftprojects" ("nftproject_id"),
  CONSTRAINT "pricelistdiscounts_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=1763 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for projectaddressestxhashes
-- ----------------------------
DROP TABLE IF EXISTS `projectaddressestxhashes`;
CREATE TABLE "projectaddressestxhashes" (
  "id" int NOT NULL AUTO_INCREMENT,
  "txhash" varchar(255) NOT NULL,
  "created" datetime NOT NULL,
  "lovelace" bigint NOT NULL DEFAULT '0',
  "tokens" varchar(255) NOT NULL DEFAULT '',
  "address" varchar(255) NOT NULL DEFAULT '',
  PRIMARY KEY ("id"),
  UNIQUE KEY "txhash" ("txhash") USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=794258 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for promotions
-- ----------------------------
DROP TABLE IF EXISTS `promotions`;
CREATE TABLE "promotions" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "nft_id" int DEFAULT NULL,
  "count" bigint NOT NULL,
  "state" enum('active','notactive') CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "startdate" datetime DEFAULT NULL,
  "enddate" datetime DEFAULT NULL,
  "soldcount" bigint NOT NULL,
  "maxcount" bigint DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "promotions_nftprojects" ("nftproject_id") USING BTREE,
  KEY "promotions_nfts" ("nft_id") USING BTREE,
  CONSTRAINT "promotions_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "promotions_nfts" FOREIGN KEY ("nft_id") REFERENCES "nfts" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for ratelimit
-- ----------------------------
DROP TABLE IF EXISTS `ratelimit`;
CREATE TABLE "ratelimit" (
  "id" int NOT NULL AUTO_INCREMENT,
  "apikey" varchar(255) NOT NULL,
  "created" datetime NOT NULL,
  PRIMARY KEY ("id"),
  KEY "created" ("created"),
  KEY "apikey" ("apikey")
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for rates
-- ----------------------------
DROP TABLE IF EXISTS `rates`;
CREATE TABLE "rates" (
  "id" int NOT NULL AUTO_INCREMENT,
  "created" datetime NOT NULL,
  "eurorate" float(12,4) NOT NULL,
  "usdrate" float(12,4) DEFAULT NULL,
  "jpyrate" float(12,4) DEFAULT NULL,
  "btcrate" float(20,10) DEFAULT NULL,
  "soleurorate" float(12,4) DEFAULT NULL,
  "solusdrate" float(12,4) DEFAULT NULL,
  "soljpyrate" float(12,4) DEFAULT NULL,
  "solbtcrate" float(20,10) DEFAULT NULL,
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=161517 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for referer
-- ----------------------------
DROP TABLE IF EXISTS `referer`;
CREATE TABLE "referer" (
  "id" int NOT NULL AUTO_INCREMENT,
  "referertoken" varchar(255) NOT NULL,
  "referercustomer_id" int NOT NULL,
  "state" enum('active','notactive') NOT NULL,
  "commisionpercent" float(12,2) NOT NULL,
  "payoutwallet_id" int DEFAULT NULL,
  PRIMARY KEY ("id"),
  UNIQUE KEY "referertoken" ("referertoken"),
  KEY "referer_customers" ("referercustomer_id"),
  KEY "referer_customerwallets" ("payoutwallet_id"),
  CONSTRAINT "referer_customers" FOREIGN KEY ("referercustomer_id") REFERENCES "customers" ("id") ON DELETE CASCADE,
  CONSTRAINT "referer_customerwallets" FOREIGN KEY ("payoutwallet_id") REFERENCES "customerwallets" ("id") ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for refundlogs
-- ----------------------------
DROP TABLE IF EXISTS `refundlogs`;
CREATE TABLE "refundlogs" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "senderaddress" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "receiveraddress" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "txhash" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "created" datetime NOT NULL,
  "refundreason" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "log" text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci,
  "state" enum('successful','failed') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "outgoingtxhash" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  "lovelace" bigint DEFAULT NULL,
  "fee" bigint DEFAULT NULL,
  "nmkrcosts" bigint NOT NULL DEFAULT '0',
  "coin" enum('ADA','SOL') NOT NULL DEFAULT 'ADA',
  PRIMARY KEY ("id"),
  KEY "refundlogs_nftprojects" ("nftproject_id"),
  KEY "refundslog1" ("senderaddress"),
  KEY "refundslog2" ("receiveraddress"),
  KEY "refundslog3" ("txhash"),
  KEY "refundslog4" ("outgoingtxhash"),
  CONSTRAINT "refundlogs_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=99405 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for registeredtokens
-- ----------------------------
DROP TABLE IF EXISTS `registeredtokens`;
CREATE TABLE "registeredtokens" (
  "id" int NOT NULL AUTO_INCREMENT,
  "created" datetime NOT NULL,
  "subject" varchar(255) NOT NULL,
  "policyid" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  "url" varchar(255) DEFAULT NULL,
  "name" varchar(255) DEFAULT NULL,
  "ticker" varchar(255) DEFAULT NULL,
  "decimals" int DEFAULT NULL,
  "logo" longtext,
  "description" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id"),
  KEY "subject" ("subject"),
  KEY "policyid" ("policyid")
) ENGINE=InnoDB AUTO_INCREMENT=1612 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for reservednfts
-- ----------------------------
DROP TABLE IF EXISTS `reservednfts`;
CREATE TABLE "reservednfts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nft_id" int NOT NULL,
  "reservedcount" int NOT NULL,
  "reserveduntil" datetime NOT NULL,
  "reservedforaddress" varchar(255) NOT NULL,
  PRIMARY KEY ("id"),
  KEY "reservednfts_nfts" ("nft_id"),
  CONSTRAINT "reservednfts_nfts" FOREIGN KEY ("nft_id") REFERENCES "nfts" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for serverexceptions
-- ----------------------------
DROP TABLE IF EXISTS `serverexceptions`;
CREATE TABLE "serverexceptions" (
  "id" int NOT NULL AUTO_INCREMENT,
  "logmessage" varchar(255) NOT NULL,
  "data" longtext,
  "created" datetime NOT NULL,
  "server_id" int DEFAULT NULL,
  PRIMARY KEY ("id"),
  KEY "exceptions_backgroundserver" ("server_id"),
  CONSTRAINT "exceptions_backgroundserver" FOREIGN KEY ("server_id") REFERENCES "backgroundserver" ("id") ON DELETE SET NULL ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=41305329 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for settings
-- ----------------------------
DROP TABLE IF EXISTS `settings`;
CREATE TABLE "settings" (
  "id" int NOT NULL AUTO_INCREMENT,
  "mintingcosts" bigint NOT NULL,
  "mintingaddress" varchar(255) NOT NULL,
  "mintingaddressdescription" varchar(255) DEFAULT NULL,
  "minutxo" bigint NOT NULL,
  "minfees" bigint NOT NULL,
  "metadatascaffold" longtext NOT NULL,
  "description" varchar(255) DEFAULT NULL,
  "minimumtxcount" int NOT NULL DEFAULT '0',
  "mastersettings_id" int DEFAULT NULL,
  "feespercent" float(10,2) NOT NULL DEFAULT '1.00',
  "uploadsourceforceprice" varchar(255) DEFAULT NULL COMMENT 'When an upload source was  passed by the api function (uploadNft), we will set the price settings of the project to this setting',
  "mintandsendcoupons" int NOT NULL DEFAULT '1',
  "mintingcostssolana" bigint NOT NULL DEFAULT '0',
  "mintingaddresssolana" varchar(255) NOT NULL,
  "pricemintcoupons" bigint NOT NULL,
  "priceupdatenfts" float(12,2) NOT NULL,
  "pricemintcouponssolana" bigint NOT NULL DEFAULT '0',
  "pricemintcouponsaptos" bigint NOT NULL DEFAULT '0',
  "mintingcostsaptos" bigint NOT NULL DEFAULT '0',
  "Mintingaddresssaptos" varchar(255) NOT NULL,
  "mintingcostsbitcoin" bigint NOT NULL DEFAULT '0',
  "mintingaddressbitcoin" varchar(255) NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "settings_settings" ("mastersettings_id") USING BTREE,
  CONSTRAINT "settings_settings" FOREIGN KEY ("mastersettings_id") REFERENCES "settings" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=38 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for sftpgenericfiles
-- ----------------------------
DROP TABLE IF EXISTS `sftpgenericfiles`;
CREATE TABLE "sftpgenericfiles" (
  "id" int NOT NULL AUTO_INCREMENT,
  "title" varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "content" longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "created" datetime NOT NULL,
  "state" enum('active','notactive') CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "mimetype" varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for smartcontracts
-- ----------------------------
DROP TABLE IF EXISTS `smartcontracts`;
CREATE TABLE "smartcontracts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "smartcontractname" varchar(255) NOT NULL,
  "filename" varchar(255) NOT NULL,
  "hashaddress" varchar(255) NOT NULL,
  "state" enum('active','notactive') NOT NULL,
  "type" enum('auction','directsale','directsaleV2','directsaleoffer','cip68') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "address" varchar(255) NOT NULL,
  "sourcecode" longtext,
  "plutus" longtext,
  "timevalue" bigint DEFAULT NULL,
  "memvalue" bigint DEFAULT NULL,
  "defaultproject_id" int DEFAULT NULL COMMENT 'The project we will use if no other project is specified. The project is only for the settings',
  PRIMARY KEY ("id"),
  KEY "smartcontracts_projects" ("defaultproject_id"),
  CONSTRAINT "smartcontracts_projects" FOREIGN KEY ("defaultproject_id") REFERENCES "nftprojects" ("id") ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for smartcontractsjsontemplates
-- ----------------------------
DROP TABLE IF EXISTS `smartcontractsjsontemplates`;
CREATE TABLE "smartcontractsjsontemplates" (
  "id" int NOT NULL AUTO_INCREMENT,
  "smartcontracts_id" int NOT NULL,
  "templatetype" varchar(255) NOT NULL,
  "jsontemplate" text NOT NULL,
  "redeemertemplate" text,
  "recipienttemplate" text NOT NULL,
  PRIMARY KEY ("id"),
  KEY "smartcontractsjsontemplates_smartcontracts" ("smartcontracts_id"),
  CONSTRAINT "smartcontractsjsontemplates_smartcontracts" FOREIGN KEY ("smartcontracts_id") REFERENCES "smartcontracts" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for smartcontractsmarketplacesettings
-- ----------------------------
DROP TABLE IF EXISTS `smartcontractsmarketplacesettings`;
CREATE TABLE "smartcontractsmarketplacesettings" (
  "id" int NOT NULL AUTO_INCREMENT,
  "skey" text NOT NULL,
  "vkey" text NOT NULL,
  "salt" varchar(255) NOT NULL,
  "address" varchar(255) NOT NULL,
  "pkh" varchar(255) NOT NULL,
  "description" varchar(255) NOT NULL,
  "percentage" float(11,2) NOT NULL,
  "fakesignaddress" varchar(255) DEFAULT NULL,
  "fakesignvkey" varchar(255) DEFAULT NULL,
  "fakesignskey" varchar(255) DEFAULT NULL,
  "collateral" varchar(255) NOT NULL,
  PRIMARY KEY ("id") USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for soldnft
-- ----------------------------
DROP TABLE IF EXISTS `soldnft`;
CREATE TABLE "soldnft" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nft_id" int NOT NULL,
  "tokencount" int NOT NULL,
  "created" datetime NOT NULL,
  "server_id" int DEFAULT NULL,
  PRIMARY KEY ("id"),
  KEY "soldnft_backgroundserver" ("server_id"),
  KEY "soldnft_nfts" ("nft_id"),
  CONSTRAINT "soldnft_backgroundserver" FOREIGN KEY ("server_id") REFERENCES "backgroundserver" ("id") ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT "soldnft_nfts" FOREIGN KEY ("nft_id") REFERENCES "nfts" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for splitroyaltyaddresses
-- ----------------------------
DROP TABLE IF EXISTS `splitroyaltyaddresses`;
CREATE TABLE "splitroyaltyaddresses" (
  "id" int NOT NULL AUTO_INCREMENT,
  "customer_id" int NOT NULL,
  "address" varchar(255) NOT NULL,
  "skey" text NOT NULL,
  "vkey" text NOT NULL,
  "salt" varchar(255) NOT NULL,
  "created" datetime NOT NULL,
  "state" enum('active','notactive','deleted') NOT NULL,
  "minthreshold" bigint NOT NULL DEFAULT '0',
  "lastcheck" datetime DEFAULT NULL,
  "comment" varchar(255) NOT NULL,
  "lovelace" bigint NOT NULL DEFAULT '0',
  PRIMARY KEY ("id"),
  KEY "splitroyalityaddresses_customers" ("customer_id"),
  CONSTRAINT "splitroyalityaddresses_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=372 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for splitroyaltyaddressessplits
-- ----------------------------
DROP TABLE IF EXISTS `splitroyaltyaddressessplits`;
CREATE TABLE "splitroyaltyaddressessplits" (
  "id" int NOT NULL AUTO_INCREMENT,
  "splitroyaltyaddresses_id" int NOT NULL,
  "address" varchar(255) NOT NULL,
  "percentage" int NOT NULL COMMENT 'percentage * 100 / so 10 percent = 1000',
  "isMainReceiver" tinyint(1) NOT NULL DEFAULT '1',
  "state" enum('active','notactive') NOT NULL,
  "activefrom" datetime DEFAULT NULL,
  "activeto" datetime DEFAULT NULL,
  "created" datetime NOT NULL,
  PRIMARY KEY ("id"),
  KEY "splitroyaltyaddressessplits_splitroyaltyaddresses" ("splitroyaltyaddresses_id"),
  CONSTRAINT "splitroyaltyaddressessplits_splitroyaltyaddresses" FOREIGN KEY ("splitroyaltyaddresses_id") REFERENCES "splitroyaltyaddresses" ("id") ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=1285 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for splitroyaltyaddressestransactions
-- ----------------------------
DROP TABLE IF EXISTS `splitroyaltyaddressestransactions`;
CREATE TABLE "splitroyaltyaddressestransactions" (
  "id" int NOT NULL AUTO_INCREMENT,
  "splitroyaltyaddresses_id" int NOT NULL,
  "amount" bigint NOT NULL,
  "created" datetime NOT NULL,
  "changeaddress" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "fee" bigint NOT NULL,
  "txid" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "costs" bigint NOT NULL,
  "costsaddress" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "splitroyaltyaddressestransactions_splitroyaltyaddresses" ("splitroyaltyaddresses_id") USING BTREE,
  CONSTRAINT "splitroyaltyaddressestransactions_splitroyaltyaddresses" FOREIGN KEY ("splitroyaltyaddresses_id") REFERENCES "splitroyaltyaddresses" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=2085 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for splitroyaltyaddressestransactionssplits
-- ----------------------------
DROP TABLE IF EXISTS `splitroyaltyaddressestransactionssplits`;
CREATE TABLE "splitroyaltyaddressestransactionssplits" (
  "id" int NOT NULL AUTO_INCREMENT,
  "splitroyaltyaddressestransactions_id" int NOT NULL,
  "splitaddress" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "amount" bigint NOT NULL,
  "percentage" int NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "royaltyaddressestransactionssplits_royaltyaddressestransactions" ("splitroyaltyaddressestransactions_id") USING BTREE,
  CONSTRAINT "royaltyaddressestransactionssplits_royaltyaddressestransactions" FOREIGN KEY ("splitroyaltyaddressestransactions_id") REFERENCES "splitroyaltyaddressestransactions" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=2966 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for stakepoolrewards
-- ----------------------------
DROP TABLE IF EXISTS `stakepoolrewards`;
CREATE TABLE "stakepoolrewards" (
  "id" int NOT NULL AUTO_INCREMENT,
  "stakepoolid" varchar(255) NOT NULL,
  "stakepoolname" varchar(255) NOT NULL,
  "reward" bigint NOT NULL,
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for statistics
-- ----------------------------
DROP TABLE IF EXISTS `statistics`;
CREATE TABLE "statistics" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "customer_id" int NOT NULL,
  "day" int NOT NULL,
  "month" int NOT NULL,
  "year" int NOT NULL,
  "sales" int NOT NULL,
  "amount" double(20,2) NOT NULL,
  "mintingcosts" double(20,2) NOT NULL,
  "transactionfees" double(20,2) NOT NULL,
  "minutxo" double(20,2) NOT NULL,
  PRIMARY KEY ("id"),
  KEY "statistics_nftprojects" ("nftproject_id"),
  KEY "statistics_customers" ("customer_id"),
  CONSTRAINT "statistics_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "statistics_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=28555 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for storesettings
-- ----------------------------
DROP TABLE IF EXISTS `storesettings`;
CREATE TABLE "storesettings" (
  "id" int NOT NULL AUTO_INCREMENT,
  "settingsname" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "humanreadablesettingsname" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "mandantory" tinyint(1) NOT NULL,
  "settingstype" enum('string','int','color','url','email','twitterhandle','boolean','collectionlist','image','favicon','list','fontlist') CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "description" text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "page" int NOT NULL,
  "category" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "subcategory" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "listvalues" text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  "sortorder" int NOT NULL DEFAULT '0',
  "maxwidth" int DEFAULT NULL,
  "maxheight" int DEFAULT NULL,
  "maxlength" int DEFAULT NULL,
  "allowedfiletypes" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  UNIQUE KEY "storesettings" ("settingsname") USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=45 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for submissions
-- ----------------------------
DROP TABLE IF EXISTS `submissions`;
CREATE TABLE "submissions" (
  "id" int NOT NULL,
  "state" enum('waitingforsubmission','inprogress','submitted') NOT NULL,
  "matxsigned" text NOT NULL,
  "txid" varchar(255) DEFAULT NULL,
  "reservationtoken" varchar(255) DEFAULT NULL,
  "nftproject_id" int NOT NULL,
  "type" enum('nftrandom','nftspecific','smartcontractauction','smartcontractdirectsale') DEFAULT NULL,
  "processedbyserver_id" int DEFAULT NULL,
  "created" datetime NOT NULL,
  "submitted" datetime DEFAULT NULL,
  "submitresult" enum('successful','error') DEFAULT NULL,
  "submissionlogfile" text,
  PRIMARY KEY ("id"),
  KEY "submissions_nftproject" ("nftproject_id"),
  KEY "submissions_backgroundserver" ("processedbyserver_id"),
  CONSTRAINT "submissions_backgroundserver" FOREIGN KEY ("processedbyserver_id") REFERENCES "backgroundserver" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "submissions_nftproject" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for tokenrewards
-- ----------------------------
DROP TABLE IF EXISTS `tokenrewards`;
CREATE TABLE "tokenrewards" (
  "id" int NOT NULL AUTO_INCREMENT,
  "policyid" varchar(255) NOT NULL,
  "tokennameinhex" varchar(255) DEFAULT NULL,
  "reward" bigint NOT NULL,
  "state" enum('active','notactive') NOT NULL,
  "mincount" bigint NOT NULL,
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for tooltiphelpertexts
-- ----------------------------
DROP TABLE IF EXISTS `tooltiphelpertexts`;
CREATE TABLE "tooltiphelpertexts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "description" varchar(255) NOT NULL,
  "text" longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  "link" text,
  "subtitle" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id"),
  UNIQUE KEY "tooltiphelpertextsdescription" ("description")
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for transaction_nfts
-- ----------------------------
DROP TABLE IF EXISTS `transaction_nfts`;
CREATE TABLE "transaction_nfts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "transaction_id" int NOT NULL,
  "nft_id" int DEFAULT NULL,
  "mintedontransaction" tinyint(1) NOT NULL DEFAULT '0',
  "tokencount" bigint NOT NULL DEFAULT '1',
  "nftarchive_id" int DEFAULT NULL,
  "multiplier" bigint NOT NULL DEFAULT '1',
  "ispromotion" tinyint(1) NOT NULL DEFAULT '0',
  "txhash" varchar(255) DEFAULT NULL,
  "confirmed" tinyint(1) DEFAULT NULL,
  "checkforconfirmdate" datetime DEFAULT NULL,
  "transactionblockchaintime" bigint DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "transactionnfts_transactions" ("transaction_id") USING BTREE,
  KEY "transactions_nfts" ("nft_id") USING BTREE,
  KEY "transactionsnfts_nftarchives" ("nftarchive_id"),
  CONSTRAINT "transactionnfts_transactions" FOREIGN KEY ("transaction_id") REFERENCES "transactions" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "transactions_nfts" FOREIGN KEY ("nft_id") REFERENCES "nfts" ("id") ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT "transactionsnfts_nftarchives" FOREIGN KEY ("nftarchive_id") REFERENCES "nfts_archive" ("id") ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=1938567 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for transactions
-- ----------------------------
DROP TABLE IF EXISTS `transactions`;
CREATE TABLE "transactions" (
  "id" int NOT NULL AUTO_INCREMENT,
  "senderaddress" varchar(255) DEFAULT NULL,
  "receiveraddress" varchar(255) NOT NULL,
  "ada" bigint NOT NULL,
  "created" datetime NOT NULL,
  "fee" bigint DEFAULT '0',
  "transactiontype" enum('paidonftaddress','mintfromcustomeraddress','paidtocustomeraddress','paidfromnftaddress','consolitecustomeraddress','paidfailedtransactiontocustomeraddress','doublepaymentsendbacktobuyer','paidonprojectaddress','fiatpayment','mintfromnftmakeraddress','burning','decentralmintandsend','decentralmintandsale','royaltsplit','unknown','directsale','auction','buymints','refundmints') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "transactionid" varchar(255) DEFAULT NULL,
  "nftaddress_id" int DEFAULT NULL,
  "customer_id" int DEFAULT NULL,
  "projectaddress" varchar(255) DEFAULT NULL,
  "projectada" bigint DEFAULT '0',
  "mintingcostsaddress" varchar(255) DEFAULT NULL,
  "mintingcostsada" bigint DEFAULT '0',
  "nftproject_id" int DEFAULT NULL,
  "state" enum('signed','submitted','confirmed') DEFAULT NULL,
  "eurorate" float(12,4) DEFAULT NULL,
  "wallet_id" int DEFAULT NULL,
  "serverid" int DEFAULT NULL,
  "projectincomingtxhash" varchar(255) DEFAULT NULL,
  "stakereward" bigint DEFAULT '0',
  "discount" bigint DEFAULT '0',
  "referer_id" int DEFAULT NULL,
  "referer_commission" bigint DEFAULT '0',
  "originatoraddress" varchar(255) DEFAULT NULL,
  "stakeaddress" varchar(255) DEFAULT NULL,
  "confirmed" tinyint(1) NOT NULL DEFAULT '0',
  "checkforconfirmdate" datetime DEFAULT NULL,
  "cbor" text,
  "ipaddress" varchar(255) DEFAULT NULL,
  "metadata" text,
  "preparedpaymenttransaction_id" int DEFAULT NULL,
  "tokenreward" bigint DEFAULT '0',
  "nmkrcosts" bigint NOT NULL DEFAULT '0',
  "paymentmethod" enum('ADA','FIAT','ETH','SOL','APT') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'ADA',
  "nftcount" int NOT NULL DEFAULT '0',
  "telemetrytooktime" bigint DEFAULT NULL,
  "priceintokensquantity" bigint DEFAULT NULL,
  "priceintokenspolicyid" varchar(255) DEFAULT NULL,
  "priceintokenstokennamehex" varchar(255) DEFAULT NULL,
  "priceintokensmultiplier" bigint DEFAULT NULL,
  "stopresubmitting" tinyint(1) NOT NULL DEFAULT '0',
  "customerproperty" varchar(255) DEFAULT NULL,
  "discountcode" varchar(255) DEFAULT NULL,
  "coin" enum('ADA','SOL','APT','BTC') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT 'ADA',
  "incomingtxblockchaintime" bigint DEFAULT NULL,
  "transactionblockchaintime" bigint DEFAULT NULL,
  "metadatastandard" enum('cip25','cip68','solana','aptos') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  "cip68referencetokenaddress" varchar(255) DEFAULT NULL,
  "cip68referencetokenminutxo" bigint DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "transactions_nftaddresses" ("nftaddress_id") USING BTREE,
  KEY "transactions_customers" ("customer_id") USING BTREE,
  KEY "transactions_nftprojects" ("nftproject_id") USING BTREE,
  KEY "transactions_customerwallets" ("wallet_id"),
  KEY "transactiontype" ("transactiontype"),
  KEY "transactiondate" ("created"),
  KEY "transactions_referer" ("referer_id"),
  KEY "transactions_preparedpaymenttransactions" ("preparedpaymenttransaction_id"),
  KEY "transactions_txhash" ("transactionid"),
  CONSTRAINT "transactions_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "transactions_customerwallets" FOREIGN KEY ("wallet_id") REFERENCES "customerwallets" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "transactions_nftaddresses" FOREIGN KEY ("nftaddress_id") REFERENCES "nftaddresses" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "transactions_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT "transactions_preparedpaymenttransactions" FOREIGN KEY ("preparedpaymenttransaction_id") REFERENCES "preparedpaymenttransactions" ("id") ON DELETE SET NULL,
  CONSTRAINT "transactions_referer" FOREIGN KEY ("referer_id") REFERENCES "referer" ("id") ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=1100799 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for transactions_additionalpayouts
-- ----------------------------
DROP TABLE IF EXISTS `transactions_additionalpayouts`;
CREATE TABLE "transactions_additionalpayouts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "transaction_id" int NOT NULL,
  "payoutaddress" varchar(255) NOT NULL,
  "lovelace" bigint NOT NULL,
  PRIMARY KEY ("id"),
  KEY "transactions_additionalpayouts_transactions" ("transaction_id"),
  CONSTRAINT "transactions_additionalpayouts_transactions" FOREIGN KEY ("transaction_id") REFERENCES "transactions" ("id") ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=17894 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for txhashcache
-- ----------------------------
DROP TABLE IF EXISTS `txhashcache`;
CREATE TABLE "txhashcache" (
  "id" int NOT NULL AUTO_INCREMENT,
  "txhash" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "transactionobject" text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "created" datetime NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  UNIQUE KEY "txhash" ("txhash") USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=1123 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for updateprojectsid
-- ----------------------------
DROP TABLE IF EXISTS `updateprojectsid`;
CREATE TABLE "updateprojectsid" (
  "id" int NOT NULL,
  "dummyid" bigint unsigned NOT NULL AUTO_INCREMENT,
  PRIMARY KEY ("dummyid"),
  KEY "id" ("id")
) /*!50100 STORAGE MEMORY */ ENGINE=InnoDB AUTO_INCREMENT=22657675 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for usedaddressesonsaleconditions
-- ----------------------------
DROP TABLE IF EXISTS `usedaddressesonsaleconditions`;
CREATE TABLE "usedaddressesonsaleconditions" (
  "id" int NOT NULL AUTO_INCREMENT,
  "salecondtions_id" int NOT NULL,
  "address" varchar(255) NOT NULL,
  "created" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY ("id"),
  UNIQUE KEY "usedaddressesaddress" ("address","salecondtions_id") USING BTREE,
  KEY "usedaddresses_saleconditions" ("salecondtions_id"),
  CONSTRAINT "usedaddresses_saleconditions" FOREIGN KEY ("salecondtions_id") REFERENCES "nftprojectsaleconditions" ("id") ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=459 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for validationaddresses
-- ----------------------------
DROP TABLE IF EXISTS `validationaddresses`;
CREATE TABLE "validationaddresses" (
  "id" int NOT NULL AUTO_INCREMENT,
  "address" varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "privateskey" longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "privatevkey" longtext CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "password" varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  "state" enum('active','notactive') CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  PRIMARY KEY ("id")
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for validationamounts
-- ----------------------------
DROP TABLE IF EXISTS `validationamounts`;
CREATE TABLE "validationamounts" (
  "id" int NOT NULL AUTO_INCREMENT,
  "validationaddress_id" int NOT NULL,
  "lovelace" bigint NOT NULL,
  "state" enum('notvalidated','validated','expired') NOT NULL,
  "senderaddress" varchar(255) DEFAULT NULL,
  "validuntil" datetime NOT NULL,
  "uid" varchar(255) NOT NULL,
  "optionalvalidationname" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id"),
  UNIQUE KEY "lovelace" ("validationaddress_id","lovelace"),
  UNIQUE KEY "validationamountsuid" ("uid"),
  KEY "validationamounts_validationaddresses" ("validationaddress_id"),
  CONSTRAINT "validationamounts_validationaddresses" FOREIGN KEY ("validationaddress_id") REFERENCES "validationaddresses" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=1172 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for vestingoffers
-- ----------------------------
DROP TABLE IF EXISTS `vestingoffers`;
CREATE TABLE "vestingoffers" (
  "id" int NOT NULL AUTO_INCREMENT,
  "periodindays" int NOT NULL,
  "iagonenabled" tinyint(1) NOT NULL,
  "maxfilesize" bigint NOT NULL,
  "maxfiles" bigint NOT NULL,
  "maxstorage" bigint NOT NULL,
  "extendedapienabled" tinyint(1) NOT NULL,
  "vesttokenpolicyid" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "vesttokenassetname" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "vesttokenquantity" bigint NOT NULL,
  "vesttokenada" bigint NOT NULL,
  "description" varchar(255) NOT NULL,
  PRIMARY KEY ("id") USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for websitelog
-- ----------------------------
DROP TABLE IF EXISTS `websitelog`;
CREATE TABLE "websitelog" (
  "id" int NOT NULL AUTO_INCREMENT,
  "servername" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "customer_id" int NOT NULL,
  "parameter" varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  "function" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "created" datetime NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "websitelog_customers" ("customer_id") USING BTREE,
  CONSTRAINT "websitelog_customers" FOREIGN KEY ("customer_id") REFERENCES "customers" ("id") ON DELETE CASCADE ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=1324057 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for websitesettings
-- ----------------------------
DROP TABLE IF EXISTS `websitesettings`;
CREATE TABLE "websitesettings" (
  "id" int NOT NULL AUTO_INCREMENT,
  "key" varchar(255) NOT NULL,
  "boolvalue" tinyint(1) DEFAULT NULL,
  "stringvalue" text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci,
  PRIMARY KEY ("id"),
  UNIQUE KEY "webseitesettingskey" ("key")
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8mb3;

-- ----------------------------
-- Table structure for whitelabelstorecollections
-- ----------------------------
DROP TABLE IF EXISTS `whitelabelstorecollections`;
CREATE TABLE "whitelabelstorecollections" (
  "id" int NOT NULL AUTO_INCREMENT,
  "nftproject_id" int NOT NULL,
  "storesettings_id" int NOT NULL,
  "policyid" varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "state" enum('active','notactive') NOT NULL,
  "collectionname" varchar(255) DEFAULT NULL,
  "collectiondescription" varchar(255) DEFAULT NULL,
  "nameofcreator" varchar(255) DEFAULT NULL,
  "twritterlink" varchar(255) DEFAULT NULL,
  "instagramlink" varchar(255) DEFAULT NULL,
  "discordlink" varchar(255) DEFAULT NULL,
  "activefrom" datetime DEFAULT NULL,
  "activeto" datetime DEFAULT NULL,
  "showonfrontpage" tinyint(1) NOT NULL DEFAULT '1',
  "isdropinprogess" tinyint(1) DEFAULT '0',
  "dropprojectuid" varchar(255) DEFAULT NULL,
  "uid" varchar(255) DEFAULT NULL,
  "previewimage" varchar(255) DEFAULT NULL,
  PRIMARY KEY ("id") USING BTREE,
  KEY "whitelabelstorecollections_nftprojects" ("nftproject_id") USING BTREE,
  KEY "whitelabelstorecollections_storesettings" ("storesettings_id") USING BTREE,
  CONSTRAINT "whitelabelstorecollections_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "whitelabelstorecollections_storesettings" FOREIGN KEY ("storesettings_id") REFERENCES "storesettings" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=1232 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Table structure for whitelabelstoresettings
-- ----------------------------
DROP TABLE IF EXISTS `whitelabelstoresettings`;
CREATE TABLE "whitelabelstoresettings" (
  "id" int NOT NULL AUTO_INCREMENT,
  "storesettings_id" int NOT NULL,
  "value" text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  "nftproject_id" int NOT NULL,
  PRIMARY KEY ("id") USING BTREE,
  UNIQUE KEY "storesettings_stores" ("storesettings_id","nftproject_id") USING BTREE,
  KEY "whitelabelstoresettings_nftprojects" ("nftproject_id") USING BTREE,
  CONSTRAINT "whitelabelstoresettings_nftprojects" FOREIGN KEY ("nftproject_id") REFERENCES "nftprojects" ("id") ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT "whitelabelstoresettings_storesettings" FOREIGN KEY ("storesettings_id") REFERENCES "storesettings" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=7921 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- View structure for backgroundtasklogview
-- ----------------------------
DROP VIEW IF EXISTS `backgroundtasklogview`;
CREATE ALGORITHM = UNDEFINED SQL SECURITY DEFINER VIEW `backgroundtasklogview` AS select "backgroundtaskslog"."id" AS "id","backgroundtaskslog"."logmessage" AS "logmessage","backgroundtaskslog"."created" AS "created" from "backgroundtaskslog" order by "backgroundtaskslog"."id" desc limit 0,100;

-- ----------------------------
-- View structure for getaddressesfordoublepayment
-- ----------------------------
DROP VIEW IF EXISTS `getaddressesfordoublepayment`;
CREATE ALGORITHM = UNDEFINED SQL SECURITY DEFINER VIEW `getaddressesfordoublepayment` AS select "nftaddresses"."id" AS "id","nftaddresses"."address" AS "address","nftaddresses"."lastcheckforutxo" AS "lastcheckforutxo","nftaddresses"."paydate" AS "paydate","nftaddresses"."state" AS "state","nftaddresses"."created" AS "created","nftaddresses"."checkfordoublepayment" AS "checkfordoublepayment","nftaddresses"."coin" AS "coin" from "nftaddresses" where (((("nftaddresses"."state" = 'expired') or ("nftaddresses"."state" = 'error') or ("nftaddresses"."state" = 'rejected') or ("nftaddresses"."state" = 'error2') or ("nftaddresses"."state" = 'paid')) and (("nftaddresses"."paydate" < (now() - interval 1 hour)) or ("nftaddresses"."paydate" is null)) and ("nftaddresses"."created" > (now() - interval 4 day))) or ("nftaddresses"."checkfordoublepayment" = 1)) order by "nftaddresses"."lastcheckforutxo";

-- ----------------------------
-- View structure for getallmetadataplaceholder
-- ----------------------------
DROP VIEW IF EXISTS `getallmetadataplaceholder`;
CREATE ALGORITHM = UNDEFINED SQL SECURITY DEFINER VIEW `getallmetadataplaceholder` AS select "nftprojects"."id" AS "id","nfts"."name" AS "name","metadata"."placeholdername" AS "placeholdername","metadata"."placeholdervalue" AS "placeholdervalue" from (("nftprojects" join "nfts" on(("nftprojects"."id" = "nfts"."nftproject_id"))) join "metadata" on(("nfts"."id" = "metadata"."nft_id")));

-- ----------------------------
-- View structure for getidsforpolicycheck
-- ----------------------------
DROP VIEW IF EXISTS `getidsforpolicycheck`;
CREATE ALGORITHM = UNDEFINED SQL SECURITY DEFINER VIEW `getidsforpolicycheck` AS select "nfts"."id" AS "id","nfts"."mintedonblockchain" AS "mintedonblockchain" from "nfts" where (((("nfts"."soldcount" > 0) and ("nfts"."fingerprint" is null)) or ("nfts"."checkpolicyid" = 1)) and ("nfts"."isroyaltytoken" = 0) and ("nfts"."selldate" < (now() - interval 20 minute)) and ("nfts"."selldate" > (now() - interval 2 day))) order by "nfts"."selldate" desc;

-- ----------------------------
-- View structure for getlimit
-- ----------------------------
DROP VIEW IF EXISTS `getlimit`;
CREATE ALGORITHM = UNDEFINED SQL SECURITY DEFINER VIEW `getlimit` AS select count(0) AS "rate","ratelimit"."apikey" AS "apikey" from "ratelimit" where ("ratelimit"."created" > (now() - interval 1 minute)) group by "ratelimit"."apikey";

-- ----------------------------
-- View structure for getprojectstatisticsview
-- ----------------------------
DROP VIEW IF EXISTS `getprojectstatisticsview`;
CREATE ALGORITHM = UNDEFINED SQL SECURITY DEFINER VIEW `getprojectstatisticsview` AS select "transactions"."nftproject_id" AS "nftproject_id","nftprojects"."projectname" AS "projectname","nftprojects"."customer_id" AS "customer_id","transactions"."transactiontype" AS "transactiontype",count(0) AS "totaltransactions",(sum("transactions"."ada") / 1000000) AS "totalsendbacktousers",(sum(("transactions"."ada" * "transactions"."eurorate")) / 1000000) AS "totalsendbacktouserseuro",(sum("transactions"."fee") / 1000000) AS "totalfees",(sum(("transactions"."fee" * "transactions"."eurorate")) / 1000000) AS "totalfeeseuro",(sum("transactions"."mintingcostsada") / 1000000) AS "totalmintingcosts",(sum(("transactions"."mintingcostsada" * "transactions"."eurorate")) / 1000000) AS "totalmintingcostseuro",(sum("transactions"."projectada") / 1000000) AS "totalpayout",(sum(("transactions"."projectada" * "transactions"."eurorate")) / 1000000) AS "totalpayouteuro","transactions"."coin" AS "coin" from (("transactions" left join "transaction_nfts" on(("transactions"."id" = "transaction_nfts"."transaction_id"))) left join "nftprojects" on(("transactions"."nftproject_id" = "nftprojects"."id"))) where (("transactions"."transactiontype" = 'paidonprojectaddress') or ("transactions"."transactiontype" = 'paidonftaddress') or ("transactions"."transactiontype" = 'mintfromcustomeraddress')) group by "transactions"."nftproject_id","transactions"."transactiontype","transactions"."coin";

-- ----------------------------
-- View structure for getstatecounts
-- ----------------------------
DROP VIEW IF EXISTS `getstatecounts`;
CREATE ALGORITHM = UNDEFINED SQL SECURITY DEFINER VIEW `getstatecounts` AS select count(0) AS "c","nfts"."nftproject_id" AS "nftproject_id","nfts"."state" AS "state",coalesce(sum("nfts"."reservedcount"),0) AS "tokensreserved",coalesce(sum("nfts"."soldcount"),0) AS "tokenssold",coalesce(sum("nfts"."errorcount"),0) AS "tokenserror",(count(0) * (select "nftprojects"."maxsupply" from "nftprojects" where ("nftprojects"."id" = "nfts"."nftproject_id"))) AS "total" from "nfts" where (("nfts"."mainnft_id" is null) and ("nfts"."isroyaltytoken" = 0)) group by "nfts"."nftproject_id","nfts"."state" order by "nfts"."nftproject_id","nfts"."state";

-- ----------------------------
-- View structure for getstatisticsview
-- ----------------------------
DROP VIEW IF EXISTS `getstatisticsview`;
CREATE ALGORITHM = UNDEFINED SQL SECURITY DEFINER VIEW `getstatisticsview` AS select count(0) AS "totaltransactions","transactions"."coin" AS "coin","transactions"."transactiontype" AS "transactiontype",(sum("transactions"."ada") / 1000000) AS "totalsendbacktousers",(sum(("transactions"."ada" * "transactions"."eurorate")) / 1000000) AS "totalsendbacktouserseuro",(sum("transactions"."fee") / 1000000) AS "totalfees",(sum(("transactions"."fee" * "transactions"."eurorate")) / 1000000) AS "totalfeeseuro",(sum("transactions"."mintingcostsada") / 1000000) AS "totalmintingcosts",(sum(("transactions"."mintingcostsada" * "transactions"."eurorate")) / 1000000) AS "totalmintingcostseuro",(sum("transactions"."projectada") / 1000000) AS "totalpayout",(sum(("transactions"."projectada" * "transactions"."eurorate")) / 1000000) AS "totalpayouteuro",count("transaction_nfts"."id") AS "totalnfts",(sum("transactions"."nmkrcosts") / 1000000) AS "totalnmkrcosts",(sum(("transactions"."nmkrcosts" * "transactions"."eurorate")) / 1000000) AS "totalnmkrcostseuro",(select if(((select "customers"."country_id" from "customers" where ("customers"."id" = "transactions"."customer_id")) = 206),'CH','ROW')) AS "countryselect" from ("transactions" left join "transaction_nfts" on(("transactions"."id" = "transaction_nfts"."transaction_id"))) where (("transactions"."transactiontype" <> 'consolitecustomeraddress') and ("transactions"."transactiontype" <> 'paidtocustomeraddress')) group by "transactions"."transactiontype","countryselect","transactions"."coin";

-- ----------------------------
-- View structure for nftprojects_view
-- ----------------------------
DROP VIEW IF EXISTS `nftprojects_view`;
CREATE ALGORITHM = UNDEFINED SQL SECURITY DEFINER VIEW `nftprojects_view` AS select "nftprojects"."id" AS "id","nftprojects"."customer_id" AS "customer_id","nftprojects"."projectname" AS "projectname","nftprojects"."payoutaddress" AS "payoutaddress","nftprojects"."policyscript" AS "policyscript","nftprojects"."policyaddress" AS "policyaddress","nftprojects"."policyid" AS "policyid","nftprojects"."policyvkey" AS "policyvkey","nftprojects"."policyskey" AS "policyskey","nftprojects"."policyexpire" AS "policyexpire","nftprojects"."state" AS "state","nftprojects"."password" AS "password","nftprojects"."tokennameprefix" AS "tokennameprefix","nftprojects"."settings_id" AS "settings_id","nftprojects"."expiretime" AS "expiretime","nftprojects"."customerwallet_id" AS "customerwallet_id","nftprojects"."description" AS "description","nftprojects"."maxsupply" AS "maxsupply","nftprojects"."version" AS "version","nftprojects"."minutxo" AS "minutxo","nftprojects"."metadata" AS "metadata","nftprojects"."oldmetadatascheme" AS "oldmetadatascheme","nftprojects"."lastupdate" AS "lastupdate","nftprojects"."projecturl" AS "projecturl","nftprojects"."hasroyality" AS "hasroyality","nftprojects"."royalitypercent" AS "royalitypercent","nftprojects"."royalityaddress" AS "royalityaddress","nftprojects"."royaltiycreated" AS "royaltiycreated","nftprojects"."activatepayinaddress" AS "activatepayinaddress",(select count(0) from "nfts" where (("nfts"."nftproject_id" = "nftprojects"."id") and ("nfts"."mainnft_id" is null) and ("nfts"."state" <> 'deleted') and ("nfts"."isroyaltytoken" = 0))) AS "total",(select count(0) from "nfts" where (("nfts"."nftproject_id" = "nftprojects"."id") and ("nfts"."mainnft_id" is null) and ("nfts"."state" = 'free') and ("nfts"."isroyaltytoken" = 0))) AS "free",(select count(0) from "nfts" where (("nfts"."nftproject_id" = "nftprojects"."id") and ("nfts"."mainnft_id" is null) and ("nfts"."state" = 'reserved') and ("nfts"."isroyaltytoken" = 0))) AS "reserved",(select count(0) from "nfts" where (("nfts"."nftproject_id" = "nftprojects"."id") and ("nfts"."mainnft_id" is null) and ("nfts"."state" = 'sold') and ("nfts"."isroyaltytoken" = 0))) AS "sold",(select count(0) from "nfts" where (("nfts"."nftproject_id" = "nftprojects"."id") and ("nfts"."mainnft_id" is null) and ("nfts"."state" = 'error') and ("nfts"."isroyaltytoken" = 0))) AS "error",(select coalesce((count(0) * "nftprojects"."maxsupply"),0) from "nfts" where (("nfts"."nftproject_id" = "nftprojects"."id") and ("nfts"."mainnft_id" is null) and ("nfts"."isroyaltytoken" = 0))) AS "totaltokens",(select coalesce(sum("nfts"."soldcount"),0) from "nfts" where (("nfts"."nftproject_id" = "nftprojects"."id") and ("nfts"."mainnft_id" is null) and ("nfts"."isroyaltytoken" = 0))) AS "tokenssold",(select coalesce(sum("nfts"."reservedcount"),0) from "nfts" where (("nfts"."nftproject_id" = "nftprojects"."id") and ("nfts"."mainnft_id" is null) and ("nfts"."isroyaltytoken" = 0))) AS "tokensreserved",(select count(0) from "pricelist" where (("pricelist"."nftproject_id" = "nftprojects"."id") and ("pricelist"."state" = 'active'))) AS "countprices" from "nftprojects";

-- ----------------------------
-- View structure for salenumbers
-- ----------------------------
DROP VIEW IF EXISTS `salenumbers`;
CREATE ALGORITHM = UNDEFINED SQL SECURITY DEFINER VIEW `salenumbers` AS select count(0) AS "soldnfts",(select count(0) from "transaction_nfts") AS "sold",((select sum("transactions"."projectada") from "transactions" where (("transactions"."transactiontype" = 'paidonftaddress') or ("transactions"."transactiontype" = 'paidonprojectaddress'))) / 1000000) AS "ada",(select "rates"."eurorate" from "rates" order by "rates"."id" desc limit 0,1) AS "eurorate" from "nfts" where ("nfts"."state" = 'sold');

-- ----------------------------
-- View structure for transactionstatistics
-- ----------------------------
DROP VIEW IF EXISTS `transactionstatistics`;
CREATE ALGORITHM = UNDEFINED SQL SECURITY DEFINER VIEW `transactionstatistics` AS select count(0) AS "counttx",(sum(ifnull("transactions"."ada",0)) / 1000000) AS "sumada",(sum(ifnull("transactions"."fee",0)) / 1000000) AS "sumfee",(sum(ifnull("transactions"."projectada",0)) / 1000000) AS "sumprojectada",(sum(ifnull("transactions"."mintingcostsada",0)) / 1000000) AS "summintcosts",((((sum(ifnull("transactions"."ada",0)) + sum(ifnull("transactions"."projectada",0))) + sum(ifnull("transactions"."mintingcostsada",0))) + sum(ifnull("transactions"."fee",0))) / 1000000) AS "sumtotal",(((sum(ifnull("transactions"."ada",0)) + sum(ifnull("transactions"."mintingcostsada",0))) + sum(ifnull("transactions"."fee",0))) / 1000000) AS "sumcosts","transactions"."customer_id" AS "customer_id","transactions"."nftproject_id" AS "nftproject_id",cast("transactions"."created" as date) AS "d1" from "transactions" where ("transactions"."transactiontype" = 'paidonftaddress') group by "transactions"."customer_id","transactions"."nftproject_id",cast("transactions"."created" as date);

-- ----------------------------
-- Procedure structure for AddApiLog
-- ----------------------------
DROP PROCEDURE IF EXISTS `AddApiLog`;
delimiter ;;
CREATE PROCEDURE `AddApiLog`(IN apifunction VARCHAR(255), IN nftproject_id INT, IN exceedratelimit TINYINT)
BEGIN
  

END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for AddLimit
-- ----------------------------
DROP PROCEDURE IF EXISTS `AddLimit`;
delimiter ;;
CREATE PROCEDURE `AddLimit`(IN api_key VARCHAR(255))
BEGIN
  insert into ratelimit (apikey,created) VALUES(api_key,NOW()); 
  delete from ratelimit where created < DATE_ADD(NOW(), INTERVAL -5 MINUTE); 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for debug_msg
-- ----------------------------
DROP PROCEDURE IF EXISTS `debug_msg`;
delimiter ;;
CREATE PROCEDURE `debug_msg`(enabled INTEGER, msg VARCHAR(255))
BEGIN
  IF enabled THEN
    select concat('** ', msg) AS '** DEBUG:'; 
  END IF; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for GetNftNumbers
-- ----------------------------
DROP PROCEDURE IF EXISTS `GetNftNumbers`;
delimiter ;;
CREATE PROCEDURE `GetNftNumbers`(IN nftprojectid int)
BEGIN
    select count(*) as totaltokens, COALESCE(sum(soldcount),0) as soldcount,COALESCE(sum(reservedcount),0) as reservedcount,COALESCE(sum(errorcount),0) as errorcount from nfts where nftproject_id=nftprojectid; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for GetNftNumbersByGroup
-- ----------------------------
DROP PROCEDURE IF EXISTS `GetNftNumbersByGroup`;
delimiter ;;
CREATE PROCEDURE `GetNftNumbersByGroup`(IN nftprojectid int, IN groupid int)
BEGIN
    select count(*) as totaltokens, COALESCE(sum(soldcount),0) as soldcount,COALESCE(sum(reservedcount),0) as reservedcount,COALESCE(sum(errorcount),0) as errorcount from nfts where nftproject_id=nftprojectid and nftgroup_id = groupid; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for GetProjectStatistics
-- ----------------------------
DROP PROCEDURE IF EXISTS `GetProjectStatistics`;
delimiter ;;
CREATE PROCEDURE `GetProjectStatistics`(IN fromdate date,IN todate date)
BEGIN
select 
transactions.nftproject_id,
nftprojects.projectname,
nftprojects.customer_id,
transactions.transactiontype,
count(0) AS `totaltransactions`,
(sum(`transactions`.`ada`) / (IF(coin='SOL',1000000000,1000000))) AS `totalsendbacktousers`,
(sum((`transactions`.`ada` * `transactions`.`eurorate`)) / (IF(coin='SOL',1000000000,1000000))) AS `totalsendbacktouserseuro`,
(sum(`transactions`.`fee`) / (IF(coin='SOL',1000000000,1000000))) AS `totalfees`,
(sum((`transactions`.`fee` * `transactions`.`eurorate`)) / (IF(coin='SOL',1000000000,1000000))) AS `totalfeeseuro`,
(sum(`transactions`.`mintingcostsada`) / (IF(coin='SOL',1000000000,1000000))) AS `totalmintingcosts`,
(sum((`transactions`.`mintingcostsada` * `transactions`.`eurorate`)) / (IF(coin='SOL',1000000000,1000000))) AS `totalmintingcostseuro`,
(sum(`transactions`.`projectada`) / (IF(coin='SOL',1000000000,1000000))) AS `totalpayout`,
(sum((`transactions`.`projectada` * `transactions`.`eurorate`)) / (IF(coin='SOL',1000000000,1000000))) AS `totalpayouteuro`,
(sum(`transactions`.`nmkrcosts`) / (IF(coin='SOL',1000000000,1000000))) AS `totalnmkrcosts`,
(sum((`transactions`.`nmkrcosts` * `transactions`.`eurorate`)) / (IF(coin='SOL',1000000000,1000000))) AS `totalnmkrcostseuro`,
coin

		
from (`transactions` 
left join nftprojects on ((transactions.nftproject_id = nftprojects.id)))
where 
transactions.created >= fromdate AND
	transactions.created < todate and confirmed=1 AND 
(transactions.transactiontype!='paidfailedtransactiontocustomeraddress' and 
	transactions.transactiontype!= 'doublepaymentsendbacktobuyer' and 
	transactions.transactiontype!= 'paidtocustomeraddress' and 
	transactions.transactiontype!= 'consolitecustomeraddress' and
	transactions.transactiontype!='unknown')

group by `transactions`.`nftproject_id`, transactiontype, coin; 



END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for GetProjectView
-- ----------------------------
DROP PROCEDURE IF EXISTS `GetProjectView`;
delimiter ;;
CREATE PROCEDURE `GetProjectView`(IN projectid INT)
BEGIN
 SELECT
	*,  
	( SELECT count(*) FROM nfts WHERE nftproject_id = projectid AND mainnft_id IS NULL AND state <> 'deleted' and isroyaltytoken=0 ) AS total,
	( SELECT count(*) FROM nfts WHERE nftproject_id = projectid AND mainnft_id IS NULL AND state = 'free' and isroyaltytoken=0 ) AS free,
	( SELECT count(*) FROM nfts WHERE nftproject_id = projectid AND mainnft_id IS NULL AND state = 'reserved' and isroyaltytoken=0 ) AS reserved,
	( SELECT count(*) FROM nfts WHERE nftproject_id = projectid AND mainnft_id IS NULL AND state = 'sold' and isroyaltytoken=0 ) AS sold,
	( SELECT count(*) FROM nfts WHERE nftproject_id = projectid AND mainnft_id IS NULL AND state = 'error' and isroyaltytoken=0 ) AS error,
	( SELECT COALESCE(count(*) * nftprojects.maxsupply,0) FROM nfts WHERE nftproject_id = projectid AND mainnft_id IS NULL and isroyaltytoken=0 ) AS totaltokens,
	( SELECT COALESCE(sum( soldcount ),0) FROM nfts WHERE nftproject_id = projectid AND mainnft_id IS NULL and isroyaltytoken=0 ) AS tokenssold,
	( SELECT COALESCE(sum( reservedcount ),0) FROM nfts WHERE nftproject_id = projectid AND mainnft_id IS NULL and isroyaltytoken=0 ) AS tokensreserved,
( SELECT count(*) from pricelist where nftproject_id=projectid and state='active') as countprices

FROM
	nftprojects 
	where id=projectid; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for GetStatistics
-- ----------------------------
DROP PROCEDURE IF EXISTS `GetStatistics`;
delimiter ;;
CREATE PROCEDURE `GetStatistics`(IN fromdate date,IN todate date)
BEGIN
 SELECT
	count(*) as totaltransactions, 
	transactiontype, 
	sum(ada)/(IF(coin='SOL',1000000000,1000000)) as totalsendbacktousers, 
	sum(ada*eurorate)/(IF(coin='SOL',1000000000,1000000)) as totalsendbacktouserseuro, 
	sum(fee)/(IF(coin='SOL',1000000000,1000000)) as totalfees,
	sum(fee*eurorate)/(IF(coin='SOL',1000000000,1000000)) as totalfeeseuro, 
	sum(mintingcostsada)/(IF(coin='SOL',1000000000,1000000)) as totalmintingcosts, 
	sum(mintingcostsada*eurorate)/(IF(coin='SOL',1000000000,1000000)) as totalmintingcostseuro, 
	sum(projectada)/(IF(coin='SOL',1000000000,1000000)) as totalpayout,
	sum(projectada*eurorate)/(IF(coin='SOL',1000000000,1000000)) as totalpayouteuro,
	(sum(`transactions`.`nmkrcosts`) / (IF(coin='SOL',1000000000,1000000))) AS `totalnmkrcosts`,
 (sum((`transactions`.`nmkrcosts` * `transactions`.`eurorate`)) / (IF(coin='SOL',1000000000,1000000))) AS `totalnmkrcostseuro`,
	
	 sum(transactions.nftcount) as totalnfts,
			(select IF((select country_id from customers where id=transactions.customer_id)=206,'CH','ROW')) as countryselect,
			coin
	
FROM
	transactions
	
WHERE
	created >= fromdate AND confirmed=1 AND 
	created < todate and 
	(transactions.transactiontype!='paidfailedtransactiontocustomeraddress' and 
	transactions.transactiontype!= 'doublepaymentsendbacktobuyer' and 
	transactions.transactiontype!= 'paidtocustomeraddress' and 
		transactions.transactiontype!= 'unknown' and 
	transactions.transactiontype!= 'consolitecustomeraddress')
GROUP BY
	transactiontype,countryselect,coin;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for kill_all_sleep_connections
-- ----------------------------
DROP PROCEDURE IF EXISTS `kill_all_sleep_connections`;
delimiter ;;
CREATE PROCEDURE `kill_all_sleep_connections`()
BEGIN
  WHILE (SELECT count(*) as _count from information_schema.processlist where Command = 'Sleep' and (User='api-preprod' or User='notifyservice-preprod')) >= 1 DO
    set @c := (SELECT concat('CALL mysql.rds_kill(', id, ');') as c from information_schema.processlist where Command = 'Sleep' and (User='api-preprod' or User='notifyservice-preprod') limit 1);
    prepare stmt from @c;
    execute stmt;
  END WHILE;
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for MarkAsError
-- ----------------------------
DROP PROCEDURE IF EXISTS `MarkAsError`;
delimiter ;;
CREATE PROCEDURE `MarkAsError`(IN token VARCHAR(255))
BEGIN
   
   DECLARE resid int; 
	 DECLARE tokencount bigint; 
	 DECLARE nftid int; 
	  DECLARE max_supply bigint; 
	 DECLARE project_id int; 
   DECLARE done INT DEFAULT 0; 
   DECLARE cur1 CURSOR FOR SELECT id FROM nftreservations as t1 where reservationtoken=token; 
	 DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1; 

OPEN cur1; 

REPEAT
    FETCH cur1 INTO resid; 
  	
	 IF NOT done THEN
 		
	    select tc,nft_id into @tokencount,@nftid from nftreservations where id=resid; 
	    select nftproject_id into project_id from nfts where id=@nftid; 
	    SELECT maxsupply into max_supply from nftprojects where id=project_id; 
			
			if (@nftid is not null and @tokencount is not null) THEN
			  if (max_supply=1) THEN
		          update nfts set state='error', reserveduntil=null, markedaserror=NOW(), reservedcount=0, errorcount=1 where id=@nftid and state='reserved'; 
					 ELSE
					    UPDATE nfts set reservedcount=reservedcount - @tokencount, errorcount=errorcount+@tokencount where id = @nftid; 
		     END IF; 
	       
				 INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('Marked as error from Stored Procedure: ',@nftid) ,NOW()); 
			END IF; 
   END IF; 
		 
UNTIL done END REPEAT; 
  CLOSE cur1; 
  DELETE from nftreservations where reservationtoken=token; 
	INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('Delete from nftreservations - Token:',token) ,NOW()); 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for MarkAsSold
-- ----------------------------
DROP PROCEDURE IF EXISTS `MarkAsSold`;
delimiter ;;
CREATE PROCEDURE `MarkAsSold`(IN token VARCHAR(255))
BEGIN
   
   DECLARE resid int; 
	 DECLARE tokencount bigint; 
	 DECLARE nftid int; 
	 DECLARE max_supply bigint; 
	 DECLARE project_id int; 
	 DECLARE premintedaddress int; 
   DECLARE done INT DEFAULT 0; 
   DECLARE cur1 CURSOR FOR SELECT id FROM nftreservations as t1 where reservationtoken=token; 
	 DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1; 

OPEN cur1; 

REPEAT
    FETCH cur1 INTO resid; 
  	
	 IF NOT done THEN
 		
	    select tc,nft_id into @tokencount,@nftid from nftreservations where id=resid; 
	    select nftproject_id into project_id from nfts where id=@nftid; 
	    SELECT maxsupply into max_supply from nftprojects where id=project_id; 
	
			if (@nftid is not null and @tokencount is not null) THEN
			   if (max_supply=1) THEN
		       
					 select instockpremintedaddress_id into premintedaddress from nfts where id=@nftid; 
					 if (premintedaddress is not null) THEN
					    update premintednftsaddresses set state='free',expires=null,lovelace=0,nftproject_id=null where id=premintedaddress; 
					 end if; 
					 INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('Set NFT ',@nftid, ' as sold:',token) ,NOW()); 
					 update nfts set state='sold', reserveduntil=null, selldate=NOW(), instockpremintedaddress_id=null where id=@nftid; 
					 
		     END IF; 
			
	       UPDATE nfts set reservedcount=GREATEST(reservedcount - @tokencount, 0), soldcount=soldcount+@tokencount where id = @nftid; 
				 
				 if (max_supply>1) THEN
			  	  INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('From Stored Procedure - Max-Supply:',max_supply,'Set Check Policy Id') ,NOW()); 
            UPDATE nfts set checkpolicyid=1, lastpolicycheck=null where id=@nftid; 
						/* when all sold, mark as sold */ 
						UPDATE nfts set state='sold', selldate=NOW() where id=@nftid and soldcount>=max_supply; 
				 END IF; 
				 
				 INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('Marked ', @tokencount, ' as sold from Stored Procedure: ',@nftid, ' - Token:',token) ,NOW()); 

			ELSE
				 INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('ERROR WHILE MARKING AS SOLD - Token:',token) ,NOW()); 
			END IF; 
   END IF; 
		 
UNTIL done END REPEAT; 
  CLOSE cur1; 
  DELETE from nftreservations where reservationtoken=token; 
	INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('Delete from nftreservations - Token:',token) ,NOW()); 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for ReleaseNfts
-- ----------------------------
DROP PROCEDURE IF EXISTS `ReleaseNfts`;
delimiter ;;
CREATE PROCEDURE `ReleaseNfts`(IN token VARCHAR(255))
BEGIN
	 DECLARE resid int; 
	 DECLARE tokencount bigint; 
	 DECLARE nftid int; 
	 DECLARE max_supply bigint; 
	 DECLARE project_id int; 
	 DECLARE c1 int; 
	 DECLARE statex VARCHAR(255); 
   DECLARE done INT DEFAULT 0; 
   DECLARE cur1 CURSOR FOR SELECT id FROM nftreservations as t1 where reservationtoken=token; 
	 DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1; 

OPEN cur1; 

REPEAT
    FETCH cur1 INTO resid; 
  	
	 IF NOT done THEN
 		
	    select tc,nft_id into @tokencount,@nftid from nftreservations where id=resid; 
	    select nftproject_id into project_id from nfts where id=@nftid; 
	    SELECT maxsupply into max_supply from nftprojects where id=project_id; 
	
			if (@nftid is not null and @tokencount is not null) THEN
				 if (max_supply=1) THEN
				    select state into @statex from nfts where id=@nftid; 
				    if (@statex='reserved') then
						   select count(*) into c1 from nftreservations where nft_id=@nftid; 
						 	 if (c1<=1) then
			           update nfts set state='free', reserveduntil=null, reservedcount=0 where id=@nftid and state='reserved'; 
							  INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('Released 1 to free from Stored Procedure: ',@nftid, ' - Token:',token,' - Count:',@tokencount) ,NOW()); 
							ELSE
								 INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('NOT Released 1 to free from Stored Procedure: ',@nftid, ' - Token:',token,' - Count:',@tokencount,' because reserved elsewhere ',c1) ,NOW()); 
							 END IF; 
				/*		else
							 INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('NOT Released 1 to free from Stored Procedure: ',@nftid, ' - Token:',token,' - Count:',@tokencount,' because it is not marked as reserved ',c1) ,NOW());*/
						end if; 
				 ELSE
					   UPDATE nfts set reservedcount=GREATEST(reservedcount - @tokencount, 0) where id = @nftid; 
						 INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('Released to free from Stored Procedure: ',@nftid, ' - Token:',token,' - Count:',@tokencount) ,NOW()); 
			   END IF; 
				 
			END IF; 
   END IF; 
		 
UNTIL done END REPEAT; 
   CLOSE cur1; 
   DELETE from nftreservations where reservationtoken=token; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for ReserveAddress
-- ----------------------------
DROP PROCEDURE IF EXISTS `ReserveAddress`;
delimiter ;;
CREATE PROCEDURE `ReserveAddress`()
BEGIN
   DECLARE nid int(11); 
   select id into nid from nftaddresses where state='free' and nftproject_id is null limit 0,1; 

if (nid is not null) then
   update nftaddresses set state='reserved' where id=nid; 
   select * from nftaddresses where id=nid; 
else
   select * from nftaddresses where id=0; 
end if; 


END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for ReserveAddress2
-- ----------------------------
DROP PROCEDURE IF EXISTS `ReserveAddress2`;
delimiter ;;
CREATE PROCEDURE `ReserveAddress2`(IN token VARCHAR(255))
BEGIN
   DECLARE nid int(11); 
   select id into nid from nftaddresses where state='free' and nftproject_id is null and reservationtoken is null limit 0,1; 

if (nid is not null) then
   update nftaddresses set state='reserved', reservationtoken=token where id=nid and state='free' and reservationtoken is null; 
   select * from nftaddresses where id=nid and state='reserved' and reservationtoken=token; 
else
   select * from nftaddresses where id=0; 
end if; 


END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for ReserveNftRandom
-- ----------------------------
DROP PROCEDURE IF EXISTS `ReserveNftRandom`;
delimiter ;;
CREATE PROCEDURE `ReserveNftRandom`(IN token VARCHAR(255), IN project_id int, IN tokencount int, IN reservationtime int)
BEGIN
  DECLARE x INT; 
  DECLARE tc1 INT; 
	DECLARE selectedserver INT; 
	 DECLARE tmp VARCHAR(255); 
	 
	 set tmp=CONCAT('Stored Procedure called ReserveNftRandom - Token:',token,' - Count:',tokencount,'- ProjectId:',project_id,'- Reservationtime:',reservationtime); 
	 if (tmp is not null) then
		   	INSERT INTO backgroundtaskslog (logmessage,created) VALUES (tmp ,NOW()); 
		else
			 INSERT INTO backgroundtaskslog (logmessage,created) VALUES ('ERROR: Some Variables from ReseveNftRandom were NULL (1)' ,NOW()); 
		end if; 


	select id into selectedserver from backgroundserver where state='active' and checkpaymentaddresses=1 order by rand() limit 0,1; 
  SET x = 0; 
  forloop: LOOP
  IF x >= tokencount THEN
  LEAVE forloop; 
  END IF; 
    SET x = x + 1; 
    
	  call ReserveNftRandomSingleNew(token,project_id,reservationtime,selectedserver); 

  END LOOP; 

  SELECT SUM(tc) into tc1 from nftreservations where reservationtoken=token; 
	if (tc1 < tokencount) THEN
	
	  set tmp=CONCAT('Not enough tokens or nfts available from stored procedure random - Token:',token,' - Count:',tokencount); 
	  if (tmp is not null) then
		   	INSERT INTO backgroundtaskslog (logmessage,created) VALUES (tmp ,NOW()); 
		else
			 INSERT INTO backgroundtaskslog (logmessage,created) VALUES ('ERROR: Some Variables from ReseveNftRandom were NULL (2)' ,NOW()); 
		end if; 
		
	   call ReleaseNfts(token); 
	END IF; 
 
  SELECT * from nftreservations where reservationtoken=token; 

 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for ReserveNftRandomByGroup
-- ----------------------------
DROP PROCEDURE IF EXISTS `ReserveNftRandomByGroup`;
delimiter ;;
CREATE PROCEDURE `ReserveNftRandomByGroup`(IN token VARCHAR(255), IN project_id int, IN tokencount int, IN reservationtime int, IN groupid int)
BEGIN
  DECLARE x INT; 
  DECLARE tc1 INT; 
	DECLARE selectedserver INT; 
	 DECLARE tmp VARCHAR(255); 
	 
	 set tmp=CONCAT('Stored Procedure called ReserveNftRandom - Token:',token,' - Count:',tokencount,'- ProjectId:',project_id,'- Reservationtime:',reservationtime, '-Group',groupid); 
	 if (tmp is not null) then
		   	INSERT INTO backgroundtaskslog (logmessage,created) VALUES (tmp ,NOW()); 
		else
			 INSERT INTO backgroundtaskslog (logmessage,created) VALUES ('ERROR: Some Variables from ReserveNftRandomByGroup were NULL (1)' ,NOW()); 
		end if; 


	select id into selectedserver from backgroundserver where state='active' and checkpaymentaddresses=1 order by rand() limit 0,1; 
  SET x = 0; 
  forloop: LOOP
  IF x >= tokencount THEN
  LEAVE forloop; 
  END IF; 
    SET x = x + 1; 
    
	  call ReserveNftRandomSingleByGroup(token,project_id,reservationtime,selectedserver,groupid); 

  END LOOP; 

  SELECT SUM(tc) into tc1 from nftreservations where reservationtoken=token; 
	if (tc1 < tokencount) THEN
	
	  set tmp=CONCAT('Not enough tokens or nfts available from stored procedure random - Token:',token,' - Count:',tokencount); 
	  if (tmp is not null) then
		   	INSERT INTO backgroundtaskslog (logmessage,created) VALUES (tmp ,NOW()); 
		else
			 INSERT INTO backgroundtaskslog (logmessage,created) VALUES ('ERROR: Some Variables from ReserveNftRandomByGroup were NULL (2)' ,NOW()); 
		end if; 
		
	   call ReleaseNfts(token); 
	END IF; 
 
  SELECT * from nftreservations where reservationtoken=token; 

 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for ReserveNftRandomMintAndSend
-- ----------------------------
DROP PROCEDURE IF EXISTS `ReserveNftRandomMintAndSend`;
delimiter ;;
CREATE PROCEDURE `ReserveNftRandomMintAndSend`(IN token VARCHAR(255), IN project_id int, IN tokencount int, IN reservationtime int)
BEGIN
  DECLARE x INT; 
  DECLARE tc1 INT; 
	DECLARE selectedserver INT; 
	 DECLARE tmp VARCHAR(255); 
	 
	 set tmp=CONCAT('Stored Procedure called ReserveNftRandom - Token:',token,' - Count:',tokencount,'- ProjectId:',project_id,'- Reservationtime:',reservationtime); 
	 if (tmp is not null) then
		   	INSERT INTO backgroundtaskslog (logmessage,created) VALUES (tmp ,NOW()); 
		else
			 INSERT INTO backgroundtaskslog (logmessage,created) VALUES ('ERROR: Some Variables from ReseveNftRandom were NULL (1)' ,NOW()); 
		end if; 


	select id into selectedserver from backgroundserver where state='active' and checkpaymentaddresses=1 order by rand() limit 0,1; 
  SET x = 0; 
  forloop: LOOP
  IF x >= tokencount THEN
  LEAVE forloop; 
  END IF; 
    SET x = x + 1; 
    
	  call ReserveNftRandomSingleNewMintAndSend(token,project_id,reservationtime,selectedserver); 

  END LOOP; 

  SELECT SUM(tc) into tc1 from nftreservations where reservationtoken=token; 
	if (tc1 < tokencount) THEN
	
	  set tmp=CONCAT('Not enough tokens or nfts available from stored procedure random - Token:',token,' - Count:',tokencount); 
	  if (tmp is not null) then
		   	INSERT INTO backgroundtaskslog (logmessage,created) VALUES (tmp ,NOW()); 
		else
			 INSERT INTO backgroundtaskslog (logmessage,created) VALUES ('ERROR: Some Variables from ReseveNftRandom were NULL (2)' ,NOW()); 
		end if; 
		
	   call ReleaseNfts(token); 
	END IF; 
 
  SELECT * from nftreservations where reservationtoken=token; 

 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for ReserveNftRandomSingle
-- ----------------------------
DROP PROCEDURE IF EXISTS `ReserveNftRandomSingle`;
delimiter ;;
CREATE PROCEDURE `ReserveNftRandomSingle`(IN token VARCHAR(255), IN project_id int, IN reservationtime int, IN selectedserver int)
BEGIN
   DECLARE done INT DEFAULT 0; 
	 DECLARE tco INT DEFAULT 0; 
	 DECLARE found INT; 
	 DECLARE nftid int; 
	 DECLARE max_supply bigint; 
   DECLARE s1 VARCHAR(255); 
	 
   DECLARE cur1 CURSOR FOR SELECT id FROM nfts as t1 where soldcount + reservedcount + errorcount < max_supply and state='free' and nftproject_id=project_id and mainnft_id is null ORDER BY RAND() LIMIT 1; 
	 DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1; 
	 SELECT maxsupply into max_supply from nftprojects where id=project_id; 
	 
	 /* Opens the Random Select and calculatues the rows */
OPEN cur1; 
Select FOUND_ROWS() into found; 

/* For Each */
IF (found = 1) THEN 
 REPEAT
    FETCH cur1 INTO nftid; 
  
	 IF NOT done THEN
	    select state into s1 from nfts where id =nftid; 
	    /* Check again */
	    if (s1 = 'free') then		
			   if (max_supply=1) THEN
			        update nfts set state='reserved', reserveduntil=DATE_ADD(NOW(), INTERVAL @reservationtime MINUTE), reservedcount=1 where id=nftid and state='free';	 
			   ELSE
		       	update nfts set reservedcount=reservedcount+1 where id=nftid and reservedcount<max_supply;	 
			   END IF; 
			   if (ROW_COUNT() = 1) THEN 
		   	   SELECT tc INTO tco from nftreservations where reservationtoken=token and nft_id=nftid; 
	   		   if (tco is null) THEN
		   	      set tco=0; 
	   		   END IF; 
		        SET tco=tco+1; 
			
			
		   	   REPLACE INTO nftreservations (nft_id,tc,reservationtoken,reservationdate,serverid) values (nftid,tco,token,now(),selectedserver); 
					 if (max_supply=1) THEN
		   	      INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('NFT Reserved from Stored Procedure (Random Single): ',nftid, ' - Token:',token,' - Project:',project_id) ,NOW()); 
					 END IF; 
			   ELSE
		   	   INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('NFT Reservation from Stored Procedure was not successful (Random Single): ',nftid, ' - Token:',token,' - Project:',project_id) ,NOW()); 
			   END IF; 
			end if; 
   END IF; 
		 
	  UNTIL done END REPEAT; 
END IF;		

 

  CLOSE cur1; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for ReserveNftRandomSingleByGroup
-- ----------------------------
DROP PROCEDURE IF EXISTS `ReserveNftRandomSingleByGroup`;
delimiter ;;
CREATE PROCEDURE `ReserveNftRandomSingleByGroup`(IN token VARCHAR(255), IN project_id int, IN reservationtime int, IN selectedserver int, IN groupid int)
BEGIN
   DECLARE done INT DEFAULT 0; 
	 DECLARE tco INT DEFAULT 0; 
	 DECLARE found INT; 
	 DECLARE nftid int; 
	 DECLARE max_supply bigint; 
   DECLARE s1 VARCHAR(255); 
	 DECLARE tmp VARCHAR(255); 
   
	 SELECT maxsupply into max_supply from nftprojects where id=project_id; 
	 
	 
	 SELECT id into nftid FROM nfts where soldcount + reservedcount + errorcount < max_supply and state='free' and nftproject_id=project_id and mainnft_id is null and isroyaltytoken=0 and price is null and group_id=groupid ORDER BY RAND() LIMIT 1; 

   Select FOUND_ROWS() into found; 

/* For Each */
   IF (found = 1) THEN   
	     select state into s1 from nfts where id =nftid; 
	    /* Check again */
	     if (s1 = 'free') then		
			   if (max_supply=1) THEN
			        update nfts set state='reserved', reserveduntil=DATE_ADD(NOW(), INTERVAL reservationtime MINUTE), reservedcount=1 where id=nftid and state='free' and isroyaltytoken=0 and price is null and mainnft_id is null and group_id=groupid;	 
			   ELSE
		        	update nfts set reservedcount=reservedcount+1 where id=nftid and reservedcount<max_supply and isroyaltytoken=0 and price is null and mainnft_id is null;	 
			   END IF; 
			   if (ROW_COUNT() = 1) THEN 
		   	   SELECT tc INTO tco from nftreservations where reservationtoken=token and nft_id=nftid; 
	   		   if (tco is null) THEN
		   	      set tco=0; 
	   		   END IF; 
		       SET tco=tco+1; 
			
			
		   	   REPLACE INTO nftreservations (nft_id,tc,reservationtoken,reservationdate,serverid) values (nftid,tco,token,now(),selectedserver); 
					 if (max_supply=1) THEN
					 set tmp=CONCAT('NFT Reserved from Stored Procedure (Random Single): ',nftid, ' - Token:',token,' - Project:',project_id,'-Reservationtime:',reservationtime, '-Groupid',groupid); 
					 if (tmp is not null) then
		   	      INSERT INTO backgroundtaskslog (logmessage,created) VALUES (tmp ,NOW()); 
							else
							INSERT INTO backgroundtaskslog (logmessage,created) VALUES ('ERROR: Some Variables from ReseveNftRandomSingleNew were NULL (1)' ,NOW()); 
							end if; 
					 END IF; 
			   ELSE
				 set tmp=CONCAT('NFT Reservation from Stored Procedure was not successful (Random Single): ',nftid, ' - Token:',token,' - Project:',project_id,'-Reservationtime:',reservationtime,'-Groupid',groupid); 
				 if (tmp is not null) then
		   	   INSERT INTO backgroundtaskslog (logmessage,created) VALUES (tmp ,NOW()); 
					 else
					 INSERT INTO backgroundtaskslog (logmessage,created) VALUES ('ERROR: Some Variables from ReseveNftRandomSingleNew were NULL (2)' ,NOW()); 
					 end if; 
			   END IF; 
			end if; 
   END IF;		

END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for ReserveNftRandomSingleNew
-- ----------------------------
DROP PROCEDURE IF EXISTS `ReserveNftRandomSingleNew`;
delimiter ;;
CREATE PROCEDURE `ReserveNftRandomSingleNew`(IN token VARCHAR(255), IN project_id int, IN reservationtime int, IN selectedserver int)
BEGIN
   DECLARE done INT DEFAULT 0; 
	 DECLARE tco INT DEFAULT 0; 
	 DECLARE found INT; 
	 DECLARE nftid int; 
	 DECLARE max_supply bigint; 
   DECLARE s1 VARCHAR(255); 
	 DECLARE tmp VARCHAR(255); 
   
	 START TRANSACTION; 
	 
	 SELECT maxsupply into max_supply from nftprojects where id=project_id; 
	 SELECT id,state into nftid, s1 FROM nfts where soldcount + reservedcount + errorcount < max_supply and state='free' and nftproject_id=project_id and mainnft_id is null and isroyaltytoken=0 ORDER BY RAND() LIMIT 1 FOR UPDATE; 

   Select FOUND_ROWS() into found; 

/* For Each */
   IF (found = 1) THEN   
	   /*  select state into s1 from nfts where id =nftid; */
	    /* Check again */
	     if (s1 = 'free') then		
			   if (max_supply=1) THEN
			     /*   update nfts set state='reserved', reserveduntil=DATE_ADD(NOW(), INTERVAL reservationtime MINUTE), reservedcount=1 where id=nftid and state='free' and isroyaltytoken=0 and price is null and mainnft_id is null;	 */
					   update nfts set state='reserved', reserveduntil=DATE_ADD(NOW(), INTERVAL reservationtime MINUTE), reservedcount=1 where id=nftid and state='free' and isroyaltytoken=0  and mainnft_id is null;	 
			   ELSE
		      /* 	update nfts set reservedcount=reservedcount+1 where id=nftid and reservedcount<max_supply and isroyaltytoken=0 and price is null and mainnft_id is null;	 */
							 	update nfts set reservedcount=reservedcount+1 where id=nftid and reservedcount<max_supply and isroyaltytoken=0 and mainnft_id is null;	 
			   END IF; 
			   if (ROW_COUNT() = 1) THEN 
		   	   SELECT tc INTO tco from nftreservations where reservationtoken=token and nft_id=nftid; 
	   		   if (tco is null) THEN
		   	      set tco=0; 
	   		   END IF; 
		       SET tco=tco+1; 
			
			
		   	   REPLACE INTO nftreservations (nft_id,tc,reservationtoken,reservationdate,serverid) values (nftid,tco,token,now(),selectedserver); 
					 if (max_supply=1) THEN
					 set tmp=CONCAT('NFT Reserved from Stored Procedure (Random Single): ',nftid, ' - Token:',token,' - Project:',project_id,'-Reservationtime:',reservationtime); 
					 if (tmp is not null) then
		   	      INSERT INTO backgroundtaskslog (logmessage,created) VALUES (tmp ,NOW()); 
							else
							INSERT INTO backgroundtaskslog (logmessage,created) VALUES ('ERROR: Some Variables from ReseveNftRandomSingleNew were NULL (1)' ,NOW()); 
							end if; 
					 END IF; 
			   ELSE
				 set tmp=CONCAT('NFT Reservation from Stored Procedure was not successful (Random Single): ',nftid, ' - Token:',token,' - Project:',project_id,'-Reservationtime:',reservationtime); 
				 if (tmp is not null) then
		   	   INSERT INTO backgroundtaskslog (logmessage,created) VALUES (tmp ,NOW()); 
					 else
					 INSERT INTO backgroundtaskslog (logmessage,created) VALUES ('ERROR: Some Variables from ReseveNftRandomSingleNew were NULL (2)' ,NOW()); 
					 end if; 
			   END IF; 
			end if; 
   END IF;		
   COMMIT; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for ReserveNftRandomSingleNewMintAndSend
-- ----------------------------
DROP PROCEDURE IF EXISTS `ReserveNftRandomSingleNewMintAndSend`;
delimiter ;;
CREATE PROCEDURE `ReserveNftRandomSingleNewMintAndSend`(IN token VARCHAR(255), IN project_id int, IN reservationtime int, IN selectedserver int)
BEGIN
   DECLARE done INT DEFAULT 0; 
	 DECLARE tco INT DEFAULT 0; 
	 DECLARE found INT; 
	 DECLARE nftid int; 
	 DECLARE max_supply bigint; 
   DECLARE s1 VARCHAR(255); 
	 DECLARE tmp VARCHAR(255); 
   
	 SELECT maxsupply into max_supply from nftprojects where id=project_id; 
	 
	 
	 SELECT id into nftid FROM nfts where soldcount + reservedcount + errorcount < max_supply and state='free' and nftproject_id=project_id and mainnft_id is null and isroyaltytoken=0 and price is null ORDER BY RAND() LIMIT 1; 

   Select FOUND_ROWS() into found; 

/* For Each */
   IF (found = 1) THEN   
	     select state into s1 from nfts where id =nftid; 
	    /* Check again */
	     if (s1 = 'free') then		
			   if (max_supply=1) THEN
			        update nfts set state='reserved', reserveduntil=DATE_ADD(NOW(), INTERVAL reservationtime MINUTE), reservedcount=1 where id=nftid and state='free' and isroyaltytoken=0 and price is null and mainnft_id is null;	 
			   ELSE
		        	update nfts set reservedcount=reservedcount+1 where id=nftid and reservedcount<max_supply and isroyaltytoken=0 and price is null and mainnft_id is null;	 
			   END IF; 
			   if (ROW_COUNT() = 1) THEN 
		   	   SELECT tc INTO tco from nftreservations where reservationtoken=token and nft_id=nftid; 
	   		   if (tco is null) THEN
		   	      set tco=0; 
	   		   END IF; 
		       SET tco=tco+1; 
			
			
		   	   REPLACE INTO nftreservations (nft_id,tc,reservationtoken,reservationdate,serverid,mintandsendcommand) values (nftid,tco,token,now(),selectedserver,1); 
					 if (max_supply=1) THEN
					 set tmp=CONCAT('NFT Reserved from Stored Procedure (Random Single): ',nftid, ' - Token:',token,' - Project:',project_id,'-Reservationtime:',reservationtime); 
					 if (tmp is not null) then
		   	      INSERT INTO backgroundtaskslog (logmessage,created) VALUES (tmp ,NOW()); 
							else
							INSERT INTO backgroundtaskslog (logmessage,created) VALUES ('ERROR: Some Variables from ReseveNftRandomSingleNew were NULL (1)' ,NOW()); 
							end if; 
					 END IF; 
			   ELSE
				 set tmp=CONCAT('NFT Reservation from Stored Procedure was not successful (Random Single): ',nftid, ' - Token:',token,' - Project:',project_id,'-Reservationtime:',reservationtime); 
				 if (tmp is not null) then
		   	   INSERT INTO backgroundtaskslog (logmessage,created) VALUES (tmp ,NOW()); 
					 else
					 INSERT INTO backgroundtaskslog (logmessage,created) VALUES ('ERROR: Some Variables from ReseveNftRandomSingleNew were NULL (2)' ,NOW()); 
					 end if; 
			   END IF; 
			end if; 
   END IF;		

END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for ReserveNftSpecific
-- ----------------------------
DROP PROCEDURE IF EXISTS `ReserveNftSpecific`;
delimiter ;;
CREATE PROCEDURE `ReserveNftSpecific`(IN token VARCHAR(255), IN project_id int, in nft_id int, IN tokencount bigint, IN reservationtime int)
BEGIN
   DECLARE done INT DEFAULT 0; 
	 DECLARE result1 LONGTEXT; 
	 DECLARE found INT; 
	 DECLARE nftid int; 
	 DECLARE max_supply bigint; 
	 DECLARE notfree bigint; 
	 DECLARE selectedserver INT; 
	 DECLARE sameserver INT; 
	 
 /*  DECLARE result1 LONGTEXT; */
   DECLARE cur1 CURSOR FOR SELECT id FROM nfts where id=nft_id and nftproject_id=project_id and mainnft_id is null and state='free' and isroyaltytoken=0; 
	 DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1; 
	 SELECT maxsupply into max_supply from nftprojects where id=project_id; 
	 
	
	 
	 /* Opens the Random Select and calculatues the rows */
OPEN cur1; 
Select FOUND_ROWS() into found; 

/* For Each */
IF (found=1) THEN 
    FETCH cur1 INTO nftid; 

    select soldcount+errorcount+reservedcount into @notfree from nfts where id=nftid and isroyaltytoken=0; 

		if (@notfree + tokencount <= max_supply) THEN
		
	    update nfts set reservedcount=reservedcount+tokencount where id=nftid and isroyaltytoken=0; 
			
				if (max_supply=1) THEN
			   update nfts set state='reserved', reserveduntil=DATE_ADD(NOW(), INTERVAL reservationtime MINUTE) where id=nftid and isroyaltytoken=0; 
			  END IF; 
		  /*  set result1 = CONCAT(result1,',',CONVERT(nftid, char)); */
		
		  /* If we already have a reservation with the same token, we will take the first selected server */
		   select count(*) into sameserver from nftreservations where reservationtoken=token; 
		   if (sameserver=0) THEN
	  	    select id into selectedserver from backgroundserver where state='active' and checkpaymentaddresses=1 order by rand() limit 0,1; 
			 ELSE
			    select serverid into selectedserver from nftreservations where reservationtoken=token limit 0,1; 
			 END IF; 
			 
			 
		   INSERT INTO nftreservations (nft_id,tc,reservationtoken,reservationdate,serverid) values (nftid,tokencount,token,now(),selectedserver); 
			 INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('NFT Reserved from Stored Procedure (specific): ',nftid, ' - Token:',token,'-Reservationtime:',reservationtime) ,NOW()); 
		else
	     INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('Not enough tokens available from stored procedure specific - Token:',token,' - Count:',tokencount, ' - Nft-ID: ',nft_id,'-Reservationtime:',reservationtime) ,NOW()); 
		
		END IF; 
		
	   	SELECT * from nftreservations where reservationtoken=token; 
ELSE
   		SELECT * from nftreservations where reservationtoken='x'; 
END IF;		
	 
  

  CLOSE cur1; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for ReserveNftSpecificMintAndSend
-- ----------------------------
DROP PROCEDURE IF EXISTS `ReserveNftSpecificMintAndSend`;
delimiter ;;
CREATE PROCEDURE `ReserveNftSpecificMintAndSend`(IN token VARCHAR(255), IN project_id int, in nft_id int, IN tokencount BIGINT, IN reservationtime int, IN mscommand int)
BEGIN
   DECLARE done INT DEFAULT 0; 
	 DECLARE result1 LONGTEXT; 
	 DECLARE found INT; 
	 DECLARE nftid int; 
	 DECLARE max_supply BIGINT; 
	 DECLARE notfree bigint; 
	 DECLARE selectedserver INT; 
	 DECLARE sameserver INT; 
	 
 /*  DECLARE result1 LONGTEXT; */
   DECLARE cur1 CURSOR FOR SELECT id FROM nfts where id=nft_id and nftproject_id=project_id and mainnft_id is null and state='free' and isroyaltytoken=0; 
	 DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1; 
	 SELECT maxsupply into max_supply from nftprojects where id=project_id; 
	 
	
	 
	 /* Opens the Random Select and calculatues the rows */
OPEN cur1; 
Select FOUND_ROWS() into found; 

/* For Each */
IF (found=1) THEN 
    FETCH cur1 INTO nftid; 

    select soldcount+errorcount+reservedcount into @notfree from nfts where id=nftid and isroyaltytoken=0; 

		if (@notfree + tokencount <= max_supply) THEN
		
	    update nfts set reservedcount=reservedcount+tokencount where id=nftid and isroyaltytoken=0; 
			
				if (max_supply=1) THEN
			   update nfts set state='reserved', reserveduntil=DATE_ADD(NOW(), INTERVAL reservationtime MINUTE) where id=nftid and isroyaltytoken=0; 
			  END IF; 
		  /*  set result1 = CONCAT(result1,',',CONVERT(nftid, char)); */
		
		  /* If we already have a reservation with the same token, we will take the first selected server */
		   select count(*) into sameserver from nftreservations where reservationtoken=token; 
		   if (sameserver=0) THEN
	  	    select id into selectedserver from backgroundserver where state='active' and checkpaymentaddresses=1 order by rand() limit 0,1; 
			 ELSE
			    select serverid into selectedserver from nftreservations where reservationtoken=token limit 0,1; 
			 END IF; 
			 
			 
		   INSERT INTO nftreservations (nft_id,tc,reservationtoken,reservationdate,serverid, mintandsendcommand) values (nftid,tokencount,token,now(),selectedserver, mscommand); 
			 INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('NFT Reserved from Stored Procedure (specific): ',nftid, ' - Token:',token,'-Reservationtime:',reservationtime) ,NOW()); 
		else
	     INSERT INTO backgroundtaskslog (logmessage,created) VALUES (CONCAT('Not enough tokens available from stored procedure specific - Token:',token,' - Count:',tokencount, ' - Nft-ID: ',nft_id,'-Reservationtime:',reservationtime) ,NOW()); 
		
		END IF; 
		
	   	SELECT * from nftreservations where reservationtoken=token; 
ELSE
   		SELECT * from nftreservations where reservationtoken='x'; 
END IF;		
	 
  

  CLOSE cur1; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for SearchCustomers
-- ----------------------------
DROP PROCEDURE IF EXISTS `SearchCustomers`;
delimiter ;;
CREATE PROCEDURE `SearchCustomers`(IN search1 VARCHAR(255), IN skip INT, IN take INT, IN orderby VARCHAR(255))
BEGIN
DECLARE search2 VARCHAR(255); 

if (search1<>'') THEN
     set search2=CONCAT('%',search1,'%'); 
		SET @query = CONCAT ('select * from customers where id=\'',search1,'\' or email like \'',search2,'\' or lastname like \'',search2,'\' or firstname like \'',search2,'\' or street like \'',search2,'\' or city like \'',search2,'\' or company like \'',search2,'\' ORDER BY ',orderby,' LIMIT ',take,' OFFSET ',skip); 
	else 
  	SET @query = CONCAT ('select * from customers ORDER BY ',orderby,' LIMIT ',take,' OFFSET ',skip); 
	end if; 
	  PREPARE stmt FROM @query; 
    EXECUTE stmt; 
    DEALLOCATE PREPARE stmt; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for SearchCustomersGetCount
-- ----------------------------
DROP PROCEDURE IF EXISTS `SearchCustomersGetCount`;
delimiter ;;
CREATE PROCEDURE `SearchCustomersGetCount`(IN search1 VARCHAR(255))
BEGIN
DECLARE search2 VARCHAR(255); 

if (search1<>'') THEN
     set search2=CONCAT('%',search1,'%'); 
		select count(*) as counttotal from customers where id=search1 or email like search2 or lastname like search2 or firstname like search2 or street like search2 or city like search2 or company like search2; 
	else 
	  select count(*) as counttotal from customers; 
		
	end if; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for SearchNfts
-- ----------------------------
DROP PROCEDURE IF EXISTS `SearchNfts`;
delimiter ;;
CREATE PROCEDURE `SearchNfts`(IN nftprojectid INT, IN statex VARCHAR(255), IN search1 VARCHAR(255), IN skip INT, IN take INT, IN orderby VARCHAR(255))
BEGIN
DECLARE search2 VARCHAR(255); 
DECLARE searchstate VARCHAR(255); 
 
 if (orderby='') THEN
  set orderby='id';
 END IF;
 
 if (statex<>'all') THEN
  set searchstate=CONCAT('state=\'',statex,'\''); 
ELSE
		set searchstate='state<>\'deleted\''; 
 END IF; 
		 
if (search1<>'') THEN
     set search2=CONCAT('%',search1,'%'); 
		SET @query = CONCAT ('select * from nfts FORCE INDEX (nftprojectstate2) where mainnft_id is null and ',searchstate,' and nftproject_id=',nftprojectid,' and ( id=\'',search1,'\' or name like \'',search2,'\' or displayname like \'',search2,'\' or uid = \'',search1,'\') ORDER BY ',orderby,' LIMIT ',take,' OFFSET ',skip); 
	else 
  	SET @query = CONCAT ('select * from nfts FORCE INDEX (nftprojectstate2) where mainnft_id is null and ',searchstate,' and nftproject_id=',nftprojectid,' ORDER BY ',orderby,' LIMIT ',take,' OFFSET ',skip); 
	end if; 
	  PREPARE stmt FROM @query; 
    EXECUTE stmt; 
    DEALLOCATE PREPARE stmt; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for SearchNftsGetCount
-- ----------------------------
DROP PROCEDURE IF EXISTS `SearchNftsGetCount`;
delimiter ;;
CREATE PROCEDURE `SearchNftsGetCount`(IN nftprojectid INT, IN statex VARCHAR(255), IN search1 VARCHAR(255))
BEGIN
DECLARE search2 VARCHAR(255); 
DECLARE searchstate VARCHAR(255); 


 if (statex<>'all') THEN
  set searchstate=CONCAT('state=\'',statex,'\''); 
ELSE
		set searchstate='state<>\'deleted\''; 
 END IF; 
 
if (search1<>'') THEN
     set search2=CONCAT('%',search1,'%'); 
		SET @query = CONCAT ('select count(*) as counttotal from nfts FORCE INDEX (nftprojectstate2) where mainnft_id is null and ',searchstate,' and nftproject_id=',nftprojectid,' and ( id=\'',search1,'\' or name like \'',search2,'\' or displayname like \'',search2,'\' or uid like \'',search2,'\')'); 
	else 
  	SET @query = CONCAT ('select count(*) as counttotal from nfts FORCE INDEX (nftprojectstate2) where mainnft_id is null and ',searchstate,' and nftproject_id=',nftprojectid); 
	end if; 
	  PREPARE stmt FROM @query; 
    EXECUTE stmt; 
    DEALLOCATE PREPARE stmt; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for SearchProjects
-- ----------------------------
DROP PROCEDURE IF EXISTS `SearchProjects`;
delimiter ;;
CREATE PROCEDURE `SearchProjects`(IN statex VARCHAR(255), IN search1 VARCHAR(255), IN customerid INT, IN skip INT, IN take INT, IN orderby VARCHAR(255))
BEGIN
DECLARE search2 VARCHAR(255); 
DECLARE searchstate VARCHAR(255); 
DECLARE searchcustomer VARCHAR(255); 

set searchstate=''; 
set searchcustomer=''; 

if (search1<>'') THEN
     set search2=CONCAT('%',search1,'%'); 
		 if (statex<>'all') THEN
		    set searchstate=CONCAT('state=\'',statex,'\' and'); 
		 END IF; 
		 if (customerid<>0) THEN
		     set searchcustomer=CONCAT(' customer_id=\'',customerid,'\' and '); 
		 END IF; 
		 
		SET @query = CONCAT ('SELECT nftprojects.* FROM nftprojects where ',searchstate,searchcustomer,' (id=\'',search1,'\' or projectname like \'',search2,'\' or description like \'',search2,'\' or uid like \'',search2,'\' or policyid like \'',search2,'\') ORDER BY ',orderby,' LIMIT ',take,' OFFSET ',skip); 
	else 
	  if (statex<>'all' and customerid=0) THEN
		    set searchstate=CONCAT(' where nftprojects.state=\'',statex,'\''); 
		 END IF; 
	 if (statex<>'all' and customerid<>0) THEN
		    set searchstate=CONCAT(' where nftprojects.state=\'',statex,'\' and '); 
				set searchcustomer=CONCAT(' customer_id=\'',customerid,'\''); 
		 END IF; 
		 if (statex='all' and customerid<>0) THEN
		 	set searchcustomer=CONCAT('where customer_id=\'',customerid,'\''); 
		 END IF; 
		 
  	SET @query = CONCAT ('SELECT nftprojects.* FROM nftprojects ',searchstate,searchcustomer,' ORDER BY ',orderby,' LIMIT ',take,' OFFSET ',skip); 
	end if; 
	  PREPARE stmt FROM @query; 
    EXECUTE stmt; 
    DEALLOCATE PREPARE stmt; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for SearchProjectsGetCount
-- ----------------------------
DROP PROCEDURE IF EXISTS `SearchProjectsGetCount`;
delimiter ;;
CREATE PROCEDURE `SearchProjectsGetCount`(IN statex VARCHAR(255), IN search1 VARCHAR(255), IN customerid INT)
BEGIN
DECLARE search2 VARCHAR(255); 
DECLARE searchstate VARCHAR(255); 
DECLARE searchcustomer VARCHAR(255); 

set searchstate=''; 
set searchcustomer=''; 

if (search1<>'') THEN
     set search2=CONCAT('%',search1,'%'); 
		 if (statex<>'all') THEN
		    set searchstate=CONCAT('state=\'',statex,'\' and'); 
		 END IF; 
		  if (customerid<>0) THEN
		     set searchcustomer=CONCAT(' customer_id=\'',customerid,'\' and '); 
		 END IF; 
		SET @query = CONCAT ('select count(*) as counttotal from nftprojects where ',searchstate,searchcustomer,' (id=\'',search1,'\' or projectname like \'',search2,'\' or description like \'',search2,'\' or uid like \'',search2,'\' or policyid like \'',search2,'\')'); 
	else 
	 if (statex<>'all' and customerid=0) THEN
		    set searchstate=CONCAT(' where state=\'',statex,'\''); 
		 END IF; 
	 if (statex<>'all' and customerid<>0) THEN
		    set searchstate=CONCAT(' where state=\'',statex,'\' and '); 
				set searchcustomer=CONCAT(' customer_id=\'',customerid,'\''); 
		 END IF; 
		 if (statex='all' and customerid<>0) THEN
		 	set searchcustomer=CONCAT('where customer_id=\'',customerid,'\''); 
		 END IF; 
  	SET @query = CONCAT ('select count(*) as counttotal from nftprojects ',searchstate,searchcustomer); 
	end if; 
	  PREPARE stmt FROM @query; 
    EXECUTE stmt; 
    DEALLOCATE PREPARE stmt; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for updatecountsonproject
-- ----------------------------
DROP PROCEDURE IF EXISTS `updatecountsonproject`;
delimiter ;;
CREATE PROCEDURE `updatecountsonproject`(nftprojectid int)
BEGIN
 START TRANSACTION;
  update nftprojects set total1=(SELECT count(*) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL AND state <> 'deleted' and isroyaltytoken=0),
   free1=(SELECT count(*) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL AND state = 'free' and isroyaltytoken=0),
   reserved1=(SELECT count(*) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL AND state = 'reserved' and isroyaltytoken=0),
   sold1=(SELECT count(*) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL AND state = 'sold' and isroyaltytoken=0),
   error1=(SELECT count(*) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL AND state = 'error' and isroyaltytoken=0),
   blocked1=(SELECT count(*) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL AND state = 'blocked' and isroyaltytoken=0),
	 totaltokens1=(SELECT COALESCE(count(*) * nftprojects.maxsupply,0) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL and isroyaltytoken=0),
   tokenssold1=(SELECT COALESCE(sum( soldcount ),0) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL and isroyaltytoken=0),
   tokensreserved1=(SELECT COALESCE(sum( reservedcount ),0) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL and isroyaltytoken=0)
	 	 where nftprojects.id=nftprojectid;
		 
	 delete from updateprojectsid where id=nftprojectid;
		 COMMIT; 
END
;;
delimiter ;

-- ----------------------------
-- Procedure structure for updatecountsonprojectnew
-- ----------------------------
DROP PROCEDURE IF EXISTS `updatecountsonprojectnew`;
delimiter ;;
CREATE PROCEDURE `updatecountsonprojectnew`()
BEGIN
  DECLARE resid int; 
  DECLARE done INT DEFAULT 0; 
  DECLARE cur1 CURSOR FOR SELECT DISTINCT id FROM updateprojectsid;
	DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1; 
 
 OPEN cur1; 
 START TRANSACTION;

REPEAT
    FETCH cur1 INTO resid; 
  	
	 IF NOT done THEN
 
  update nftprojects set total1=(SELECT count(*) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL AND state <> 'deleted' and isroyaltytoken=0),
   free1=(SELECT count(*) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL AND state = 'free' and isroyaltytoken=0),
   reserved1=(SELECT count(*) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL AND state = 'reserved' and isroyaltytoken=0),
   sold1=(SELECT count(*) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL AND state = 'sold' and isroyaltytoken=0),
   error1=(SELECT count(*) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL AND state = 'error' and isroyaltytoken=0),
   blocked1=(SELECT count(*) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL AND state = 'blocked' and isroyaltytoken=0),
	 totaltokens1=(SELECT COALESCE(count(*) * nftprojects.maxsupply,0) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL and isroyaltytoken=0),
   tokenssold1=(SELECT COALESCE(sum( soldcount ),0) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL and isroyaltytoken=0),
   tokensreserved1=(SELECT COALESCE(sum( reservedcount ),0) FROM nfts WHERE nftproject_id = nftprojects.id AND mainnft_id IS NULL and isroyaltytoken=0),
	 usedstorage=(select COALESCE(sum(nfts.filesize),0) from nfts WHERE nftproject_id = nftprojects.id)
	 	 where nftprojects.id=resid;
		 
	 delete from updateprojectsid where id=resid;
	 		END IF;  
UNTIL done END REPEAT; 
  CLOSE cur1; 

  COMMIT;
END
;;
delimiter ;

-- ----------------------------
-- Event structure for updateprojectscounts
-- ----------------------------
DROP EVENT IF EXISTS `updateprojectscounts`;
delimiter ;;
CREATE EVENT `updateprojectscounts`
ON SCHEDULE
EVERY '1' SECOND STARTS '2022-05-05 08:20:41'
DO BEGIN
 DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
 BEGIN
   DO RELEASE_LOCK('updateprojectscounts');
 END;
 IF GET_LOCK('updateprojectscounts', 0) THEN
 	call updatecountsonprojectnew();
  END IF;
  DO RELEASE_LOCK('updateprojectscounts');
END
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table nfts
-- ----------------------------
DROP TRIGGER IF EXISTS `deletenft`;
delimiter ;;
CREATE TRIGGER `deletenft` BEFORE DELETE ON `nfts` FOR EACH ROW BEGIN
IF OLD.mainnft_id IS NULL THEN
INSERT INTO updateprojectsid (id) VALUES (old.nftproject_id)
  ON DUPLICATE KEY UPDATE id=id;
END IF;
END
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table nfts
-- ----------------------------
DROP TRIGGER IF EXISTS `insertnft`;
delimiter ;;
CREATE TRIGGER `insertnft` AFTER INSERT ON `nfts` FOR EACH ROW BEGIN
IF NEW.mainnft_id IS NULL THEN
INSERT INTO updateprojectsid (id) VALUES (new.nftproject_id)
  ON DUPLICATE KEY UPDATE id=id;
END IF;
END
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table nfts
-- ----------------------------
DROP TRIGGER IF EXISTS `updatenft`;
delimiter ;;
CREATE TRIGGER `updatenft` AFTER UPDATE ON `nfts` FOR EACH ROW BEGIN
IF NEW.mainnft_id IS NULL THEN
  INSERT INTO updateprojectsid (id) VALUES (new.nftproject_id)
  ON DUPLICATE KEY UPDATE id=id;
END IF;
END
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table pricelist
-- ----------------------------
DROP TRIGGER IF EXISTS `deletepricecount`;
delimiter ;;
CREATE TRIGGER `deletepricecount` BEFORE DELETE ON `pricelist` FOR EACH ROW BEGIN
   if (old.state='active') THEN
      update nftprojects set countprices1=countprices1 - 1 where id=old.nftproject_id; 
	 END IF; 
END
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table pricelist
-- ----------------------------
DROP TRIGGER IF EXISTS `insertpricecount`;
delimiter ;;
CREATE TRIGGER `insertpricecount` AFTER INSERT ON `pricelist` FOR EACH ROW BEGIN
   if (new.state='active') THEN
      update nftprojects set countprices1=countprices1 + 1 where id=new.nftproject_id; 
	 END IF; 
END
;;
delimiter ;

-- ----------------------------
-- Triggers structure for table pricelist
-- ----------------------------
DROP TRIGGER IF EXISTS `updatepricelist`;
delimiter ;;
CREATE TRIGGER `updatepricelist` AFTER UPDATE ON `pricelist` FOR EACH ROW BEGIN
   if (new.state='active' and old.state='notactive') THEN
      update nftprojects set countprices1=countprices1 + 1 where id=new.nftproject_id; 
	 END IF; 
	 if (new.state='notactive' and old.state='active') THEN
      update nftprojects set countprices1=countprices1 - 1 where id=new.nftproject_id; 
	 END IF; 
END
;;
delimiter ;

SET FOREIGN_KEY_CHECKS = 1;
