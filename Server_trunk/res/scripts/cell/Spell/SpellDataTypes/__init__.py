# -*- coding: utf-8 -*-
#
"""
"""
from KBEDebug import *

from SpellDataType import SpellDataType
from .SpellExDataType import SpellExDataType
from .SpellExCurveDataType import SpellExCurveDataType

# 统一注册
SpellDataType.DATA_TYPE_MAPPING[SpellExDataType.dataType] = SpellExDataType
SpellDataType.DATA_TYPE_MAPPING[SpellExCurveDataType.dataType] = SpellExCurveDataType