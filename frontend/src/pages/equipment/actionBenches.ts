export type BenchField = {
  key: string
  label: string
}

export type BenchConfig = {
  title: string
  hint: string
  endpoint: string
  snapshot?: string
  defaults: Record<string, string>
  fields: BenchField[]
  build: (values: Record<string, string>) => unknown
  seed?: {
    label: string
    path: string
    build: (values: Record<string, string>) => unknown
  }
}

const equipment = (values: Record<string, string>) => values.equipmentCode.trim()
const succeeded = (values: Record<string, string>) => {
  const text = (values.resultFlag ?? '').trim().toLowerCase()
  return text !== 'false' && text !== '0'
}

export const actionBenches: BenchConfig[] = [
  {
    title: 'ClearEquipHeartBeatStatus',
    hint: '只清理这一台设备上还没完成的执行参数、物料上下机、零部件上下机和删批，把状态改成 3。不改心跳标志，也不改控机、工单、通道。先放一条上料，再点发送，未完成条数应从 1 变成 0。',
    endpoint: '/api/CZ2Mes/ClearEquipHeartBeatStatus',
    snapshot: 'clear',
    defaults: { software: '试验台上位机', equipmentCode: 'TEST-HB-01' },
    fields: [
      { key: 'software', label: '上位机名' },
      { key: 'equipmentCode', label: '设备号' },
    ],
    build: (values) => ({
      Software: values.software.trim(),
      EquipmentInfo: [{ EquipmentCode: equipment(values) }],
    }),
    seed: {
      label: '先放一条上料',
      path: '/api/equipment-bench/seed-material-load',
      build: (values) => ({ EquipmentCode: equipment(values) }),
    },
  },
  {
    title: 'EquipStatus',
    hint: '设备上报 RUN 或 DOWN。RUN 记成 Running，其它记成 Stopping。同一设备状态变了会结束上一段停机并另起一段。运行请求读的就是这里写下的状态。',
    endpoint: '/api/CZ2Mes/EquipStatus',
    snapshot: 'status',
    defaults: {
      software: '试验台上位机',
      equipmentCode: 'TEST-HB-01',
      employeeNo: 'E001',
      equipmentModel: 'RUN',
      equipmentStatusID: '1',
      reasonCode: '',
      description: '试验台上报运行',
    },
    fields: [
      { key: 'software', label: '上位机名' },
      { key: 'equipmentCode', label: '设备号' },
      { key: 'employeeNo', label: '工号' },
      { key: 'equipmentModel', label: '运行状态 RUN 或 DOWN' },
      { key: 'equipmentStatusID', label: '状态代码' },
      { key: 'reasonCode', label: '停机原因，可用短横线分层' },
      { key: 'description', label: '说明' },
    ],
    build: (values) => ({
      Software: values.software.trim(),
      EquipmentInfo: [
        {
          EquipmentCode: equipment(values),
          EmployeeNo: values.employeeNo.trim(),
          EquipmentModel: values.equipmentModel.trim(),
          EquipmentStatusID: values.equipmentStatusID.trim(),
          ReasonCode: values.reasonCode.trim(),
          Description: values.description.trim(),
        },
      ],
    }),
  },
  {
    title: 'EquipAlert',
    hint: '报警标识 1 是报警，0 是复位。同一设备、同一报警代码如果两天内还有未关闭记录，再报一次会被跳过。没有报警记录就复位，会返回「复位的报警信息不存在」。',
    endpoint: '/api/CZ2Mes/EquipAlert',
    snapshot: 'alert',
    defaults: {
      software: '试验台上位机',
      equipmentCode: 'TEST-HB-01',
      employeeNo: 'E001',
      alertCode: 'AL-01',
      alertReset: '1',
      alertDescription: '试验台报警',
      alertLevel: 'B',
    },
    fields: [
      { key: 'software', label: '上位机名' },
      { key: 'equipmentCode', label: '设备号' },
      { key: 'employeeNo', label: '工号' },
      { key: 'alertCode', label: '报警代码' },
      { key: 'alertReset', label: '报警标识，1 报警 / 0 复位' },
      { key: 'alertDescription', label: '报警描述' },
      { key: 'alertLevel', label: '报警等级' },
    ],
    build: (values) => ({
      Software: values.software.trim(),
      EquipmentInfo: [
        {
          EquipmentCode: equipment(values),
          EmployeeNo: values.employeeNo.trim(),
          AlertInfo: [
            {
              AlertCode: values.alertCode.trim(),
              AlertReset: Number(values.alertReset.trim() || '0'),
              AlertDescription: values.alertDescription.trim(),
              AlertLevel: values.alertLevel.trim(),
            },
          ],
        },
      ],
    }),
  },
  {
    title: 'EquipRunRequest',
    hint: '设备问能不能运行。先到设备状态页上报一次。状态是 Running、StandBy 或 Stopping 就允许。还没报过状态会返回设备不存在。齐套检查这一版还没迁。',
    endpoint: '/api/CZ2Mes/EquipRunRequest',
    snapshot: 'status',
    defaults: { software: '试验台上位机', equipmentCode: 'TEST-HB-01', employeeNo: 'E001' },
    fields: [
      { key: 'software', label: '上位机名' },
      { key: 'equipmentCode', label: '设备号' },
      { key: 'employeeNo', label: '工号' },
    ],
    build: (values) => ({
      Software: values.software.trim(),
      EquipmentInfo: [{ EquipmentCode: equipment(values), EmployeeNo: values.employeeNo.trim() }],
    }),
  },
  {
    title: 'EqptParaCHNG',
    hint: '设备上报一条参数变更。参数编码为空的行不会入库。新值记成采集值，旧值记成标准值。',
    endpoint: '/api/CZ2Mes/EqptParaCHNG',
    snapshot: 'para',
    defaults: {
      software: '试验台上位机',
      equipmentCode: 'TEST-HB-01',
      employeeNo: 'E001',
      productNo: 'P-01',
      oprSequenceNo: 'OP10',
      parameterCode: 'TEMP',
      oldValue: '25',
      newValue: '30',
      description: '温度',
    },
    fields: [
      { key: 'software', label: '上位机名' },
      { key: 'equipmentCode', label: '设备号' },
      { key: 'employeeNo', label: '工号' },
      { key: 'productNo', label: '产品编号' },
      { key: 'oprSequenceNo', label: '工序编号' },
      { key: 'parameterCode', label: '参数编码' },
      { key: 'oldValue', label: '旧值' },
      { key: 'newValue', label: '新值' },
      { key: 'description', label: '参数描述' },
    ],
    build: (values) => ({
      Software: values.software.trim(),
      EquipmentInfo: [
        {
          EquipmentCode: equipment(values),
          EmployeeNo: values.employeeNo.trim(),
          ProductNo: values.productNo.trim(),
          OprSequenceNo: values.oprSequenceNo.trim(),
          ParameterInfo: [
            {
              ParameterCode: values.parameterCode.trim(),
              OldValue: values.oldValue.trim(),
              NewValue: values.newValue.trim(),
              Description: values.description.trim(),
            },
          ],
        },
      ],
    }),
  },
  {
    title: 'EqptCtrl',
    hint: '设备领走控机之后，用那一拍的回执来反馈。回执不为空时，同一回执的控机行状态改成 3。回执留空则只回成功，不改队列。',
    endpoint: '/api/CZ2Mes/EqptCtrl',
    snapshot: 'control',
    defaults: {
      software: '试验台上位机',
      equipmentCode: 'TEST-HB-01',
      receiptNumber: '',
      cmd: 'STOP',
      reasonCode: 'TEST',
      description: '设备已执行',
    },
    fields: [
      { key: 'software', label: '上位机名' },
      { key: 'equipmentCode', label: '设备号' },
      { key: 'receiptNumber', label: '回执 ReceiptNumber' },
      { key: 'cmd', label: '命令' },
      { key: 'reasonCode', label: '原因' },
      { key: 'description', label: '说明' },
    ],
    build: (values) => ({
      Software: values.software.trim(),
      EquipmentInfo: [
        {
          EquipmentCode: equipment(values),
          ReceiptNumber: values.receiptNumber.trim(),
          Cmd: values.cmd.trim(),
          ReasonCode: values.reasonCode.trim(),
          Description: values.description.trim(),
          ResultFlag: true,
          Message: '',
        },
      ],
    }),
  },
  {
    title: 'ChannelCtrlFeedback',
    hint: '化成通道启停的反馈。这一版只按设备回成功，不改通道队列。报文会记到日志里。',
    endpoint: '/api/CZ2Mes/ChannelCtrlFeedback',
    defaults: {
      software: '试验台上位机',
      equipmentCode: 'TEST-HB-01',
      channelCode: 'CH-01',
      receiptNumber: '',
      slotId: '1',
      opFlag: '1',
    },
    fields: [
      { key: 'software', label: '上位机名' },
      { key: 'equipmentCode', label: '设备号' },
      { key: 'channelCode', label: '通道' },
      { key: 'receiptNumber', label: '回执' },
      { key: 'slotId', label: '槽位' },
      { key: 'opFlag', label: '启停标识' },
    ],
    build: (values) => ({
      Software: values.software.trim(),
      EquipmentInfo: [
        {
          EquipmentCode: equipment(values),
          ChannelCode: values.channelCode.trim(),
          ReceiptNumber: values.receiptNumber.trim(),
          ResultFlag: true,
          Message: '',
          OrderSendParam: [{ SlotID: values.slotId.trim(), OPFlag: values.opFlag.trim() }],
        },
      ],
    }),
  },
  {
    title: 'EquipControl',
    hint: 'MES 把控机放进队列。请求体是数组，不包在 EquipmentInfo 里。通道这一版不写。入队后到心跳试验台用同一设备号发心跳，回包应带走命令。再打开控机反馈，填那一拍的回执。',
    endpoint: '/api/CZ2Mes/EquipControl',
    snapshot: 'control',
    defaults: {
      equipmentCode: 'TEST-HB-01',
      cmd: 'STOP',
      reasonCode: 'TEST',
      description: '试验台控机',
    },
    fields: [
      { key: 'equipmentCode', label: '设备号' },
      { key: 'cmd', label: '命令' },
      { key: 'reasonCode', label: '原因' },
      { key: 'description', label: '说明' },
    ],
    build: (values) => [
      {
        EquipmentCode: equipment(values),
        Cmd: values.cmd.trim(),
        ReasonCode: values.reasonCode.trim(),
        Description: values.description.trim(),
      },
    ],
  },
  {
    title: 'MaterialLoad',
    hint: '设备领走上料之后，用那一拍的回执来反馈。先放一条：队列状态是 2，执行参数状态是 1。结果填 true 再发送，两者都变成 3。结果填 false 时，队列仍变成 3，执行参数变成 4。在制上料这一版不写。',
    endpoint: '/api/CZ2Mes/MaterialLoad',
    snapshot: 'material-load',
    defaults: {
      software: '试验台上位机',
      equipmentCode: 'TEST-HB-01',
      receiptNumber: 'RC-ML-01',
      resultFlag: 'true',
      labelNo: 'LB-TEST',
      location: 'A1',
    },
    fields: [
      { key: 'software', label: '上位机名' },
      { key: 'equipmentCode', label: '设备号' },
      { key: 'receiptNumber', label: '回执 ReceiptNumber' },
      { key: 'resultFlag', label: '结果 true 或 false' },
      { key: 'labelNo', label: '标签号' },
      { key: 'location', label: '安装位置' },
    ],
    build: (values) => ({
      Software: values.software.trim(),
      EquipmentInfo: [
        {
          EquipmentCode: equipment(values),
          ReceiptNumber: values.receiptNumber.trim(),
          ResultFlag: succeeded(values),
          MaterialInfo: [{ LabelNo: values.labelNo.trim(), Location: values.location.trim() }],
        },
      ],
    }),
    seed: {
      label: '先放一条上料',
      path: '/api/equipment-bench/seed-material-load-feedback',
      build: (values) => ({ EquipmentCode: equipment(values), ReceiptNumber: values.receiptNumber.trim() }),
    },
  },
  {
    title: 'MaterialUnload',
    hint: '设备领走下料之后的回执。先放一条再发送。队列变成 3，执行参数变成 3。报文里的成功失败和剩余数量这一版不参与判断。在制下料不写。',
    endpoint: '/api/CZ2Mes/MaterialUnload',
    snapshot: 'material-unload',
    defaults: {
      software: '试验台上位机',
      equipmentCode: 'TEST-HB-01',
      receiptNumber: 'RC-MU-01',
      resultFlag: 'true',
      labelNo: 'LB-TEST',
      materialQuantity: '1',
      uomCode: 'EA',
    },
    fields: [
      { key: 'software', label: '上位机名' },
      { key: 'equipmentCode', label: '设备号' },
      { key: 'receiptNumber', label: '回执 ReceiptNumber' },
      { key: 'resultFlag', label: '结果 true 或 false' },
      { key: 'labelNo', label: '标签号' },
      { key: 'materialQuantity', label: '剩余数量' },
      { key: 'uomCode', label: '单位' },
    ],
    build: (values) => ({
      Software: values.software.trim(),
      EquipmentInfo: [
        {
          EquipmentCode: equipment(values),
          ReceiptNumber: values.receiptNumber.trim(),
          ResultFlag: succeeded(values),
          MaterialInfo: [
            {
              LabelNo: values.labelNo.trim(),
              MaterialQuantity: values.materialQuantity.trim(),
              UomCode: values.uomCode.trim(),
            },
          ],
        },
      ],
    }),
    seed: {
      label: '先放一条下料',
      path: '/api/equipment-bench/seed-material-unload',
      build: (values) => ({ EquipmentCode: equipment(values), ReceiptNumber: values.receiptNumber.trim() }),
    },
  },
  {
    title: 'PartLoad',
    hint: '零部件上机回执。设备号为空会返回「存在设备号为空的数据，请排查;」。结果为 true 时执行参数变成 3，为 false 时变成 4。队列都变成 3。零部件装到设备上这一版不写。',
    endpoint: '/api/CZ2Mes/PartLoad',
    snapshot: 'part-load',
    defaults: {
      software: '试验台上位机',
      equipmentCode: 'TEST-HB-01',
      receiptNumber: 'RC-PL-01',
      resultFlag: 'true',
      spartId: 'PART-01',
      partDesc: '试验零部件',
      location: 'A1',
      useLifetime: '80',
      warningLife: '20',
    },
    fields: [
      { key: 'software', label: '上位机名' },
      { key: 'equipmentCode', label: '设备号' },
      { key: 'receiptNumber', label: '回执 ReceiptNumber' },
      { key: 'resultFlag', label: '结果 true 或 false' },
      { key: 'spartId', label: '零部件号 SpartID' },
      { key: 'partDesc', label: '零部件描述' },
      { key: 'location', label: '安装位置' },
      { key: 'useLifetime', label: '剩余寿命' },
      { key: 'warningLife', label: '预警寿命' },
    ],
    build: (values) => ({
      Software: values.software.trim(),
      EquipmentInfo: [
        {
          EquipmentCode: equipment(values),
          ReceiptNumber: values.receiptNumber.trim(),
          ResultFlag: succeeded(values),
          SpartInfo: [
            {
              SpartID: values.spartId.trim(),
              PartDesc: values.partDesc.trim(),
              Location: values.location.trim(),
              UseLifetime: values.useLifetime.trim(),
              WarningLife: values.warningLife.trim(),
            },
          ],
        },
      ],
    }),
    seed: {
      label: '先放一条零部件上机',
      path: '/api/equipment-bench/seed-part-load',
      build: (values) => ({ EquipmentCode: equipment(values), ReceiptNumber: values.receiptNumber.trim() }),
    },
  },
  {
    title: 'PartUnLoad',
    hint: '零部件下机回执。先放一条再发送。队列和执行参数都变成 3。报文里的寿命不改寿命表，改寿命用旁边的零部件寿命页。',
    endpoint: '/api/CZ2Mes/PartUnLoad',
    snapshot: 'part-unload',
    defaults: {
      software: '试验台上位机',
      equipmentCode: 'TEST-HB-01',
      receiptNumber: 'RC-PU-01',
      partNo: 'PART-01',
      partDesc: '试验零部件',
      useLifetime: '60',
    },
    fields: [
      { key: 'software', label: '上位机名' },
      { key: 'equipmentCode', label: '设备号' },
      { key: 'receiptNumber', label: '回执 ReceiptNumber' },
      { key: 'partNo', label: '零部件号' },
      { key: 'partDesc', label: '零部件描述' },
      { key: 'useLifetime', label: '剩余寿命' },
    ],
    build: (values) => ({
      Software: values.software.trim(),
      EquipmentInfo: [
        {
          EquipmentCode: equipment(values),
          ReceiptNumber: values.receiptNumber.trim(),
          Partlifetimeinfo: [
            {
              PartNo: values.partNo.trim(),
              PartDesc: values.partDesc.trim(),
              UseLifetime: values.useLifetime.trim(),
            },
          ],
        },
      ],
    }),
    seed: {
      label: '先放一条零部件下机',
      path: '/api/equipment-bench/seed-part-unload',
      build: (values) => ({ EquipmentCode: equipment(values), ReceiptNumber: values.receiptNumber.trim() }),
    },
  },
  {
    title: 'PartLife',
    hint: '设备上报零部件剩余寿命。先放一条寿命，剩余寿命是 80。发送后应改成上面填的剩余寿命，并多一条履历。设备号在设备表里没有时返回「设备不存在」。零部件还没放进寿命表时仍回处理成功，剩余寿命不变。',
    endpoint: '/api/CZ2Mes/PartLife',
    snapshot: 'part-life',
    defaults: {
      software: '试验台上位机',
      equipmentCode: 'TEST-HB-01',
      employeeNo: 'E001',
      partNo: 'PART-01',
      partDesc: '试验零部件',
      useLifetime: '50',
    },
    fields: [
      { key: 'software', label: '上位机名' },
      { key: 'equipmentCode', label: '设备号' },
      { key: 'employeeNo', label: '工号' },
      { key: 'partNo', label: '零部件号' },
      { key: 'partDesc', label: '零部件描述' },
      { key: 'useLifetime', label: '剩余寿命' },
    ],
    build: (values) => ({
      Software: values.software.trim(),
      EquipmentInfo: [
        {
          EquipmentCode: equipment(values),
          EmployeeNo: values.employeeNo.trim(),
          Partlifetimeinfo: [
            {
              PartNo: values.partNo.trim(),
              PartDesc: values.partDesc.trim(),
              UseLifetime: values.useLifetime.trim(),
            },
          ],
        },
      ],
    }),
    seed: {
      label: '先放一条寿命',
      path: '/api/equipment-bench/seed-part-life',
      build: (values) => ({ EquipmentCode: equipment(values), PartNo: values.partNo.trim() }),
    },
  },
  {
    title: 'MaterialCheck',
    hint: '按标签查库存，再对当前设备工单 BOM 和工序。先放设备和物料再发送，应成功并多一条上料记录，库存上料数量记成剩余数量。不放时返回找不到设备或找不到物料。工序对不上返回物料与工序不匹配。只处理第一台设备。',
    endpoint: '/api/CZ2Mes/MaterialCheck',
    snapshot: 'material-check',
    defaults: {
      software: '试验台上位机',
      equipmentCode: 'TEST-HB-01',
      processNo: 'OP10',
      labelNo: 'LB-CHECK-01',
      location: 'A1',
    },
    fields: [
      { key: 'software', label: '上位机名' },
      { key: 'equipmentCode', label: '设备号' },
      { key: 'processNo', label: '工序号' },
      { key: 'labelNo', label: '标签号' },
      { key: 'location', label: '安装位置' },
    ],
    build: (values) => ({
      Software: values.software.trim(),
      EquipmentInfo: [
        {
          EquipmentCode: equipment(values),
          ProcessNo: values.processNo.trim(),
          MaterialInfo: [{ LabelNo: values.labelNo.trim(), Location: values.location.trim() }],
        },
      ],
    }),
    seed: {
      label: '先放设备和物料',
      path: '/api/equipment-bench/seed-material-check',
      build: (values) => ({
        EquipmentCode: equipment(values),
        LabelNo: values.labelNo.trim(),
        ProcessNo: values.processNo.trim(),
      }),
    },
  },
]

export function findActionBench(title: string) {
  return actionBenches.find((item) => item.title === title) ?? actionBenches[0]
}
