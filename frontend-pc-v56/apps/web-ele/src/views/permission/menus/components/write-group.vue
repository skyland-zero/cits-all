<!-- eslint-disable no-console -->
<script lang="ts" setup>
import type { Nullable } from '@vben/types';

import { type PropType, reactive, ref, watch } from 'vue';

import {
  ElCol,
  ElForm,
  ElFormItem,
  ElInput,
  ElInputNumber,
  ElRow,
  ElSwitch,
  ElTreeSelect,
  type FormInstance,
  type FormRules,
} from 'element-plus';

import { getApi } from '#/api/permission/menus';

const props = defineProps({
  treeSelectData: {
    type: Array as PropType<any[]>,
    default: () => [],
  },
  currentRow: {
    type: Object as PropType<Nullable<any>>,
    default: () => null,
  },
});
const formRef = ref<FormInstance>();
const formLabelWidth = '140px';
const loading = ref(false);

const initData = () => ({
  parentId: null,
  name: '',
  routeName: '',
  path: '',
  redirect: '',
  icon: '',
  order: 1,
  enabled: true,
  hideInMenu: false,
  type: 1,
});
const formData = reactive(initData());

const rules = reactive<FormRules>({
  name: [
    {
      required: true,
      message: '请输入分组名称',
      trigger: 'blur',
    },
    {
      min: 1,
      max: 50,
      message: '长度在 1 到 50 个字符',
      trigger: 'blur',
    },
  ],
  routeName: [
    {
      pattern: /^[a-zA-Z0-9_-]+$/,
      message: '路由名称只能包含英文、数字、下划线和横线',
      trigger: 'blur',
    },
  ],
});
const treeSelectProps = {
  value: 'id',
  label: 'name',
  children: 'children',
};

const fetchData = async (id: string) => {
  loading.value = true;
  const res = await getApi(id);
  Object.assign(formData, res);
  loading.value = false;
};

const setValues = (currentRow: any) => {
  Object.assign(formData, currentRow);
  if (currentRow.id) {
    fetchData(currentRow.id);
  }
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
    return formData;
  }
};

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
      label="上级分组："
      prop="parentId"
    >
      <ElTreeSelect
        v-model="formData.parentId"
        :data="treeSelectData"
        :props="treeSelectProps"
        :render-after-expand="false"
        check-strictly
        clearable
        default-expand-all
      />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="分组名称：" prop="name">
      <ElInput v-model="formData.name" autocomplete="off" />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="路由名称：" prop="routeName">
      <ElInput
        v-model="formData.routeName"
        autocomplete="off"
        placeholder="唯一的英文路由名称"
      />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="路由地址：" prop="path">
      <ElInput v-model="formData.path" autocomplete="off" />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="重定向：" prop="redirect">
      <ElInput v-model="formData.redirect" />
    </ElFormItem>
    <ElRow>
      <ElCol :span="12">
        <ElFormItem :label-width="formLabelWidth" label="图标：" prop="icon">
          <ElInput v-model="formData.icon" autocomplete="off" />
        </ElFormItem>
      </ElCol>
      <ElCol :span="12">
        <ElFormItem :label-width="formLabelWidth" label="排序：" prop="order">
          <ElInputNumber v-model="formData.order" />
        </ElFormItem>
      </ElCol>
    </ElRow>
    <ElRow>
      <ElCol :span="12">
        <ElFormItem :label-width="formLabelWidth" label="启用：" prop="enabled">
          <ElSwitch
            v-model="formData.enabled"
            active-text="是"
            inactive-text="否"
            inline-prompt
          />
        </ElFormItem>
      </ElCol>
      <ElCol :span="12">
        <ElFormItem
          :label-width="formLabelWidth"
          label="隐藏："
          prop="hideInMenu"
        >
          <ElSwitch
            v-model="formData.hideInMenu"
            active-text="是"
            inactive-text="否"
            inline-prompt
          />
        </ElFormItem>
      </ElCol>
    </ElRow>
  </ElForm>
</template>
