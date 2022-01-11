#!/bin/sh

TARGET_DIR=$(dirname $(readlink -f "${BASH_SOURCE[0]}"))

CLUSTER_CONTROLLER="python ${TARGET_DIR}/kbe/tools/server/pycluster/cluster_controller.py"

${CLUSTER_CONTROLLER} query
