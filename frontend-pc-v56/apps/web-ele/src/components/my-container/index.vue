<script lang="ts" setup>
import { Page } from '@vben/common-ui';

import {
  ElAside,
  ElContainer,
  ElDivider,
  ElFooter,
  ElHeader,
  ElMain,
} from 'element-plus';

withDefaults(
  defineProps<{
    showFooter?: Boolean;
    showHeader?: Boolean;
    showLeftAside?: Boolean;
    showTableHeader?: Boolean;
  }>(),
  {
    showHeader: () => true,
    showFooter: () => false,
    showTableHeader: () => true,
    showLeftAside: () => false,
  },
);
</script>

<template>
  <Page auto-content-height>
    <ElContainer class="my-container-main-container h-full">
      <ElAside
        v-if="showLeftAside"
        class="my-container-main-container h-full bg-white"
        width="200px"
      >
        <slot name="left-aside"></slot>
      </ElAside>
      <ElContainer class="my-container-main-container h-full">
        <ElHeader v-if="showHeader" class="bg-white">
          <slot name="header"></slot>
          <ElDivider border-style="dashed" />
        </ElHeader>
        <ElMain class="h-full w-full bg-white">
          <ElContainer class="my-container-table-container h-full w-full">
            <ElHeader v-if="showTableHeader">
              <div style="padding: 6px">
                <slot name="table-header"></slot>
              </div>
              <ElDivider border-style="dashed" />
            </ElHeader>
            <ElMain class="my-container-table-main h-full w-full">
              <slot></slot>
            </ElMain>
            <ElFooter v-if="showFooter" class="my-container-table-footer">
              <slot name="table-footer"></slot>
            </ElFooter>
          </ElContainer>
        </ElMain>
      </ElContainer>
    </ElContainer>
    <slot name="dialog"></slot>
  </Page>
</template>

<style>
.my-container-main-container .el-header {
  --el-header-height: auto;

  height: auto;
  padding: 0;
  margin: 0;
}

.my-container-main-container .el-main {
  --el-main-padding: 0;

  padding: 0;
  margin-top: 0;
  margin-right: 0;
  margin-bottom: 0;
  /* stylelint-disable-next-line declaration-block-no-redundant-longhand-properties */
  margin-left: 0;
}

.my-container-table-container .el-main {
  --el-main-padding: 0;

  padding: 0;
  margin: 0;
}

.my-container-main-container .el-divider--horizontal {
  margin: 0 !important;
}

/* 列表按钮样式 */
.my-container-table-main .el-button--small {
  padding: 0;
}

/* 列表按钮样式 */
.my-container-table-main .el-button + .el-button {
  margin: 0 !important;
}

.my-container-table-footer {
  --el-footer-height: auto !important;
}

.action-buttons {
  display: flex;
  gap: 8px; /* 按钮间距 */
}
</style>
