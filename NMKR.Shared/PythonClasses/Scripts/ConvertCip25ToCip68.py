import json
from typing import Tuple
#from unittest.mock import mock_open, patch

# constants
MAX_LENGTH = 128


def int_obj(integer: int) -> dict:
  
    # positive integers
    if integer < 0:
        raise ValueError("Integer Must Non-Negative")

    # integers only
    if not isinstance(integer, int):
        raise ValueError("Value Must be An Integer")

    # simple int object
    return {"int": integer}


def to_hex(string: str) -> str:
  
    # string only
    if not isinstance(string, str):
        raise TypeError("Input Must Be A String")

    # hex encode
    return string.encode().hex()


def byte_obj(string: str) -> dict:
  
    # string only
    if not isinstance(string, str):
        raise TypeError("Input Must Be A String")

    # if string is longer than accepted length then create list of strings
    if len(string) > MAX_LENGTH:

        # split string into length 128 segments
        string_list = [string[i: i + MAX_LENGTH]
                       for i in range(0, len(string), MAX_LENGTH)]
        list_object = []

        # loop all length 128 strings
        for value in string_list:
            list_object.append({"bytes": value})

        # list of byte objects
        return {"list": list_object}

    # simple byte object
    return {"bytes": string}


def key_obj(string: str) -> dict:

    # string only
    if not isinstance(string, str):
        raise TypeError("Input Must Be A String")

    # the string here is in ascii since its the 721 keys
    if len(string) > MAX_LENGTH // 2:
        # trim the string down
        string = string[0:MAX_LENGTH // 2]

    # simple key object
    return byte_obj(to_hex(string))


def dict_obj(data: dict, key: str) -> dict:
  
    # dict conversion
    nested_map = {"map": []}

    # test for a valid dictionary input
    try:
        data[key]

        # if its empty return empty
        if not data[key]:
            return nested_map

    # if it doesn't exist return empty
    except KeyError:
        return nested_map

    # loop all the nested keys
    for nested_key in data[key]:
        # dict of strings
        if isinstance(data[key][nested_key], str):
            nested_map["map"].append(
                {"k": key_obj((nested_key)), "v": byte_obj(to_hex(data[key][nested_key]))})

        # dict of ints
        elif isinstance(data[key][nested_key], int):
            nested_map["map"].append(
                {"k": key_obj((nested_key)), "v": int_obj(data[key][nested_key])})

        # dict of lists
        elif isinstance(data[key][nested_key], list):
            nested_map["map"].append(list_obj(data[key], nested_key))

        # dict of dicts
        elif isinstance(data[key][nested_key], dict):
            nested_map["map"].append(
                {"k": key_obj((nested_key)), "v": dict_obj(data[key], nested_key)})

        # something that isnt a standard type
        else:
            raise TypeError("Forbidden Plutus Type")

    # simple dict object
    return nested_map


def list_obj(data: dict, key: str) -> dict:
 
    # default it to the empty list object
    if len(data[key]) == 0:
        return {"k": key_obj((key)), "v": {"list": []}}

    # list of dicts
    elif isinstance(data[key][0], dict):
        list_of_dicts = []
        for i, value in enumerate(data[key]):
            list_of_dicts.append(dict_obj(data[key], i))
        return {"k": key_obj((key)), "v": {"list": list_of_dicts}}

    # list of strings
    elif isinstance(data[key][0], str):
        list_object = []
        for value in data[key]:
            list_object.append({"bytes": to_hex(value)})
        return {"k": key_obj((key)), "v": {"list": list_object}}

    # list of ints
    elif isinstance(data[key][0], int):
        list_object = []
        for value in data[key]:
            list_object.append({"int": value})
        return {"k": key_obj((key)), "v": {"list": list_object}}

    # list of lists
    elif isinstance(data[key][0], list):
        list_object = []

        # loop all the lists in the list
        for l in data[key]:
            list_object.append(list_obj({'': l}, '')['v'])
        return {"k": key_obj((key)), "v": {"list": list_object}}

    # something that isnt a standard type
    else:
        raise TypeError("Forbidden Plutus Type")


def read_metadata_file(file_path: str) -> dict:
  

    # string only
    if not isinstance(file_path, str):
        raise TypeError("File Path Must Be A String")

    # attempt file read
    try:
        #with open(file_path) as f:
        data = json.loads(file_path)
        return data
       


    # handle the case when the file does not exist
    except FileNotFoundError:
        raise FileNotFoundError("File Does Not Exist")

    # handle the case when the file contains invalid JSON content
    except json.JSONDecodeError:
        raise ValueError("Invalid JSON Content In The File")


def write_metadatum_file(file_path: str, data: dict) -> None:
  
    print("5")
    # string only
    if not isinstance(file_path, str):
        raise TypeError("File Path Must Be A String")

    # attempt file write
    try:
        with open(file_path, "w") as f:
            json.dump(data, f)

    
    # file path doesn't exist
    except OSError:
        raise OSError("Error Writing File")

    # data is bad
    except TypeError:
        raise TypeError("Error serializing data type")


def get_metadata_headers(file_path: str) -> Tuple[str, str, str]:
  
    # string only
    if not isinstance(file_path, str):
        raise TypeError("File Path Must Be A String")

    data = read_metadata_file(file_path)

    # TODO multitag or multitoken

    # single tag metadata
    tag = next(iter(data.keys())) if len(data) == 1 else None

    # has token and version
    pid = next(iter(data[str(tag)].keys())) if len(data[tag]) == 2 else None

    # just token data
    tkn = next(iter(data[tag][pid].keys())) if len(
        data[tag][pid]) == 1 else None

    # return the tuple
    return tag, pid, tkn


def create_metadatum(file_path: str,extra: str,extrahex: str, tag: str, pid: str, tkn: str, version: int) -> dict:
 
    print("3")
    # string only
    if not isinstance(file_path, str):
        raise TypeError("File Path Must Be A String")

    # parent structure
    metadatum = {
        "constructor": 0,
        "fields": []
    }

    print("3a")
    # set up default values
    version_object = int_obj(version)
    map_object = dict_obj({}, '')

    # get the data
    data = read_metadata_file(file_path)
    print("3b")

    # attempt to find the metadata
    try:
        metadata = data[tag][pid][tkn]

    # return the empty metadatum if we can't find it
    except KeyError:
        metadatum['fields'].append(map_object)
        metadatum['fields'].append(version_object)
        return metadatum

    # loop everything in the metadata
    for key in metadata:
        # string conversion
        if isinstance(metadata[key], str):
            map_object["map"].append(
                {"k": key_obj((key)), "v": byte_obj(to_hex(metadata[key]))})

        # int conversion
        elif isinstance(metadata[key], int):
            map_object["map"].append(
                {"k": key_obj((key)), "v": int_obj(metadata[key])})

        # list conversion
        elif isinstance(metadata[key], list):
            map_object["map"].append(list_obj(metadata, key))

        # dict conversion
        elif isinstance(metadata[key], dict):
            map_object["map"].append(
                {"k": key_obj((key)), "v": dict_obj(metadata, key)})

        # something that isnt a standard type
        else:
            raise TypeError("Forbidden Plutus Type")

    print("4")
    # add the fields to the metadatum
    metadatum['fields'].append(map_object)

    # check if extrafield is not empty
    if extra:
        metadatum['fields'].append(byte_obj(to_hex(extra)))

    if extrahex:
        metadatum['fields'].append(byte_obj(extrahex))

    metadatum['fields'].append(version_object)

    return metadatum


def convert_metadata(cip25metadata: str, extra : str,extrahex : str, tag: str, pid: str, tkn: str, version: int) -> str:
    print("1")
    # string only
    if not isinstance(cip25metadata, str):
        raise TypeError("Cip25 Metadata Must Be A String")

    print("2")
    datum = create_metadatum(cip25metadata,extra,extrahex, tag, pid, tkn, version)
    #write_metadatum_file(datum_path, datum)
    return json.dumps(datum)



print("Running the conversion script...")
metadata = cip25metadata
tag = '721'
pid = policyid
tkn = tokenname
try:
     extra = extrafield
except NameError:
    extra=""

try:
     extrahex = extrafieldhex
except NameError:
    extrahex=""


version = 1
print("Converting the metadata file...")
print(f"Policy ID: {pid}")
print(f"Token Name: {tkn}")
print(f"CIP-25 Metadata: {cip25metadata}")
cip68metadata = convert_metadata(metadata, extra, extrahex, tag, pid, tkn, version)