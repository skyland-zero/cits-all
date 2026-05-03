<!-- eslint-disable no-console -->
<script lang="ts" setup>
import type { Nullable } from '@vben/types';

import { onMounted, type PropType, reactive, ref, watch } from 'vue';

import {
  ElForm,
  ElFormItem,
  ElInput,
  ElMessage,
  ElOption,
  ElSelect,
  ElSwitch,
  ElTreeSelect,
  type FormInstance,
  type FormRules,
} from 'element-plus';

import { getTreeApi } from '#/api/permission/organizations';
import { getSelectedApi } from '#/api/permission/roles';

const props = defineProps({
  currentRow: {
    type: Object as PropType<Nullable<any>>,
    default: () => null,
  },
});
const formRef = ref<FormInstance>();
const formLabelWidth = '160px';
const loading = ref(true);

const initData = () => ({
  id: '',
  userName: '',
  surname: '',
  passwordHash: '',
  passwordHashRecheck: '',
  organizationUnitId: '',
  mainRoleId: '',
  isActive: true,
});
const formData = reactive(initData());

const roleSelectData = ref<any[]>([]);
const treeSelectData = ref<any[]>([]);
const treeSelectProps = {
  value: 'id',
  label: 'name',
  children: 'children',
};

const rules = reactive<FormRules>({
  organizationUnitId: [
    { required: true, message: '请选择部门', trigger: 'change' },
  ],
  userName: [
    { required: true, message: '请输入账号', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
  surname: [
    { required: true, message: '请输入姓名', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
  mainRoleId: [{ required: true, message: '请选择角色', trigger: 'change' }],
  passwordHash: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
  passwordHashRecheck: [
    { required: true, message: '请输入确认密码', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
});

const setValues = (currentRow: any) => {
  Object.assign(formData, currentRow);
};

const fetchUnitSelectData = async () => {
  loading.value = true;
  const res = await getTreeApi();
  treeSelectData.value = res.items;
  loading.value = false;
};

const fetchRoleSelectData = async () => {
  const res = await getSelectedApi();
  roleSelectData.value = res.items;
};

const submit = async () => {
  const formEl = formRef.value;
  if (!formEl) {
    return;
  }
  const valid = await formEl.validate().catch((error) => {
    console.log('valid', error);
  });
  if (valid) {
    if (
      !formData.id &&
      formData.passwordHash !== formData.passwordHashRecheck
    ) {
      ElMessage({
        message: '二次输入的密码不一致',
        type: 'error',
      });
      return;
    }
    return formData;
  }
};

onMounted(() => {
  fetchUnitSelectData();
  fetchRoleSelectData();
});

/**
 * 放在defineExpose之前，其他内容之后
 */
watch(
  () => props.currentRow,
  (currentRow) => {
    console.log('currentRow', currentRow);
    if (!currentRow) return;
    setValues(currentRow);
  },
  {
    deep: true,
    immediate: true,
  },
);

defineExpose({
  submit,
});
</script>

<template>
  <ElForm ref="formRef" :model="formData" :rules="rules" v-loading="loading">
    <ElFormItem
      :label-width="formLabelWidth"
      label="部门："
      prop="organizationUnitId"
    >
      <ElTreeSelect
        v-model="formData.organizationUnitId"
        :data="treeSelectData"
        :props="treeSelectProps"
        :render-after-expand="false"
        check-strictly
        default-expand-all
      />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="账号：" prop="userName">
      <ElInput v-model="formData.userName" autocomplete="off" />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="姓名：" prop="surname">
      <ElInput v-model="formData.surname" autocomplete="off" />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="角色：" prop="mainRoleId">
      <ElSelect
        v-model="formData.mainRoleId"
        placeholder="请选择角色"
        style="width: 100%"
      >
        <ElOption
          v-for="item in roleSelectData"
          :key="item.value"
          :label="item.label"
          :value="item.value"
        />
      </ElSelect>
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="启用：" prop="isActive">
      <ElSwitch
        v-model="formData.isActive"
        active-text="启用"
        inactive-text="禁用"
        inline-prompt
      />
    </ElFormItem>
    <div v-if="!formData.id">
      <ElFormItem
        :label-width="formLabelWidth"
        label="密码："
        prop="passwordHash"
      >
        <ElInput v-model="formData.passwordHash" autocomplete="off" />
      </ElFormItem>
      <ElFormItem
        :label-width="formLabelWidth"
        label="确认密码："
        prop="passwordHashRecheck"
      >
        <ElInput v-model="formData.passwordHashRecheck" autocomplete="off" />
      </ElFormItem>
    </div>
  </ElForm>
</template>
