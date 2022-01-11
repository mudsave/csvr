#coding: utf8

import distutils.dir_util as dir_util
import json
import os
import os.path
import shutil
import subprocess
import sys

def main():
    print ('check requirement...')
    if not check_env():
        return
    config = load()
    print_config(config)
    print ('clear output...')
    prepare_output()
    print ('compile to json...')
    src_arg = get_source_files_as_arg(config)
    print ('  - source files/paths: ' + src_arg)
    call(r'java -Xmx3200M -cp xlsx2json\target\ppgame-xlsx2json-2.0-SNAPSHOT.jar;xlsx2json\target\lib\* local.campas.ppgame.xlsx2json.Application false output ' + src_arg)
    print ('copying...')
    copy_to(*config)
    print ('done.')

def check_env():
    import re
    import struct
    py_ver = sys.version[:sys.version.find(' ')]
    py_ver += ', ' + str(struct.calcsize("P") * 8) + 'bit'
    print ('  - python version: ' + py_ver)
    java_ver = subprocess.check_output(os.path.expandvars('"%JAVA_HOME%/bin/java.exe" -version'), stderr=subprocess.STDOUT, shell=True).strip()
    java_ver = re.search(r'java version "([^"]+)"', java_ver.decode('utf8'))
    if (java_ver is None):
        sys.stderr.write('ERROR: java 1.8+ needed.')
        sys.exit(code)
    java_ver = java_ver.group(1)
    print ('  -   java version: ' + java_ver)
    if (int(re.match(r'\d+\.(\d+)\.', java_ver).group(1)) < 8):
        sys.stderr.write('ERROR: java 1.8+ needed.')
        return False
    return True

def load():
    try:
        fp = open('xlsx2json.json')
        config = json.load(fp)
        fp.close()
        return config
    except:
        return input_config()

def python_2_or_3(on_py2, on_py3, *args, **kws):
    if sys.version.startswith('2'):
        return on_py2(*args, **kws)
    else:
        return on_py3(*args, **kws)

def input_config():
    server_path = my_input('drag directory of ppgame_server here and press ENTER: ')
    client_path = my_input('drag directory of ppgame_client here and press ENTER: ')
    config = [server_path, client_path]
    fp = open('xlsx2json.json', 'w')
    json.dump(config, fp)
    fp.close()
    return config

def my_input(msg):
    return python_2_or_3(my_input_py2, my_input_py3, msg)

def my_input_py2(msg):
    return raw_input(msg)

def my_input_py3(msg):
    return input(msg)

def print_config(config):
    print ('server path: ' + config[0])
    print ('client path: ' + config[1])

def prepare_output():
    if os.path.exists('output'):
        shutil.rmtree('output')
    os.mkdir('output')

def get_source_files_as_arg(config):
    if (len(sys.argv) == 1):
        return '. ' + config[1] + r'\Tools\xlsx2py\xlsxs'
    else:
        return ' '.join('"' + v + '"'for v in sys.argv[1:])

def call(cmd, err_msg='non-zero code returned.'):
    code = subprocess.call(cmd)
    if code != 0:
        print ('ERROR: ' + err_msg)
        sys.exit(code)

def copy_to(server, client):
    dir_util.copy_tree('output', server + '')
    dir_util.copy_tree('output', client + '')
    shutil.rmtree('output')

try:
    main()
except SystemExit:
    pass
except:
    import traceback
    traceback.print_exc()
finally:
    my_input('press ENTER to quit.')
