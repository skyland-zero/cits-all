<!-- eslint-disable no-console -->
<script lang="ts" setup>
import type { Nullable } from '@vben/types';

import { onMounted, type PropType, reactive, ref, watch } from 'vue';

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
import { getTreeSelectApi } from '#/api/system/pages';

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

const pageTreeSelectData = ref<any>([]);

const initData = () => ({
  parentId: null,
  pageId: null,
  name: '',
  path: '',
  redirect: '',
  query: '',
  icon: '',
  order: 1,
  enabled: true,
  hideInMenu: false,
  affixTab: false,
  keepAlive: false,
  openInNewWindow: false,
  affixTabOrder: 1,
  iframeSrc: '',
  link: '',
  type: 2,
});
const formData = reactive(initData());

const rules = reactive<FormRules>({
  name: [
    {
      required: true,
      message: '请输入菜单名称',
      trigger: 'blur',
    },
    {
      min: 1,
      max: 50,
      message: '长度在 1 到 50 个字符',
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

const fetchPageSelectData = async () => {
  const res = await getTreeSelectApi();
  pageTreeSelectData.value = res.items;
};

const submit = async () => {
  const formEl = formRef.value;
  if (!formEl) {
    return;
  }
  const valid = await formEl.validate().catch(() => {});
  if (valid) {
    return formData;
  }
};

const onPageChanged = (data: any) => {
  console.log('onPageChanged', data);
  formData.name = data.label;
  formData.path = data.extStr1;
};

onMounted(() => {
  fetchPageSelectData();
});

/**
 * 放在defineExpose之前，其他内容之后
 */
watch(
  () => props.currentRow,
  (currentRow) => {
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
  <ElForm ref="formRef" :model="formData" :rules="rules">
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
        default-expand-all
      />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="页面：" prop="pageId">
      <ElTreeSelect
        v-model="formData.pageId"
        :data="pageTreeSelectData"
        :render-after-expand="false"
        clearable
        default-expand-all
        @current-change="onPageChanged"
      >
        <template #default="{ data: { label, extStr1 } }">
          <div class="custom-tree-node">
            <span>{{ label }}</span>
            <div>
              <ElText class="mx-1" size="small" type="primary">
                {{ extStr1 }}
              </ElText>
              <!-- <el-button link type="primary"> Append </el-button> -->
            </div>
          </div>
        </template>
      </ElTreeSelect>
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="菜单名称：" prop="name">
      <ElInput v-model="formData.name" autocomplete="off" />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="路由地址：" prop="path">
      <ElInput v-model="formData.path" autocomplete="off" />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="路由参数：" prop="query">
      <ElInput v-model="formData.query" />
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
        <ElFormItem
          :label-width="formLabelWidth"
          label="固定标签页："
          prop="affixTab"
        >
          <ElSwitch
            v-model="formData.affixTab"
            active-text="是"
            inactive-text="否"
            inline-prompt
          />
        </ElFormItem>
      </ElCol>
      <ElCol :span="12">
        <ElFormItem
          :label-width="formLabelWidth"
          label="固定标签页排序："
          prop="affixTabOrder"
        >
          <ElInputNumber v-model="formData.affixTabOrder" />
        </ElFormItem>
      </ElCol>
    </ElRow>
    <ElRow>
      <ElCol :span="12">
        <ElFormItem
          :label-width="formLabelWidth"
          label="缓存："
          prop="keepAlive"
        >
          <ElSwitch
            v-model="formData.keepAlive"
            active-text="是"
            inactive-text="否"
            inline-prompt
          />
        </ElFormItem>
      </ElCol>
      <ElCol :span="12">
        <ElFormItem
          :label-width="formLabelWidth"
          label="新窗口打开："
          prop="openInNewWindow"
        >
          <ElSwitch
            v-model="formData.openInNewWindow"
            active-text="是"
            inactive-text="否"
            inline-prompt
          />
        </ElFormItem>
      </ElCol>
    </ElRow>
    <ElFormItem
      :label-width="formLabelWidth"
      label="内嵌页面iframe："
      prop="iframeSrc"
    >
      <ElInput
        v-model="formData.iframeSrc"
        :rows="2"
        autocomplete="off"
        type="textarea"
      />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="外链:" prop="link">
      <ElInput
        v-model="formData.link"
        :rows="2"
        autocomplete="off"
        type="textarea"
      />
    </ElFormItem>
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

<style scoped>
.custom-tree-node {
  display: flex;
  flex: 1;
  align-items: center;
  justify-content: space-between;
  padding-right: 8px;
  font-size: 14px;
}
</style>
