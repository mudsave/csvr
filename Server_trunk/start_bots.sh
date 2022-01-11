#!/bin/sh

export KBE_ROOT=$(dirname $(readlink -f "${BASH_SOURCE[0]}"))
export KBE_RES_PATH=$KBE_ROOT/res/:$KBE_ROOT/kbe/res/
export KBE_BIN_PATH=$KBE_ROOT/kbe/bin/Hybrid_linux/

#nohup $KBE_BIN_PATH/bots&
$KBE_BIN_PATH/bots

