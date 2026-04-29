<script lang="ts" setup>
import { ref } from 'vue';

import { Page } from '@vben/common-ui';

import { useFullscreen } from '@vueuse/core';
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

const containerRef = ref<HTMLElement>();
const { isFullscreen, toggle: toggleFullscreen } = useFullscreen(containerRef);

defineExpose({
  isFullscreen,
  toggleFullscreen,
});
</script>

<template>
  <Page auto-content-height>
    <div ref="containerRef" class="my-container-fullscreen h-full">
      <ElContainer
        class="my-container-main-container h-full overflow-hidden rounded-xl border border-gray-100 bg-white shadow-sm"
      >
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
    </div>
    <slot name="dialog"></slot>
  </Page>
</template>

<style>
@media (max-width: 640px) {
  .my-container-main-container
    .demo-form-inline
    .el-form-item:not(:last-child) {
    width: 100%;
  }

  .my-container-main-container .demo-form-inline .el-form-item:last-child {
    width: 100%;
  }

  .my-container-main-container
    .demo-form-inline
    .el-form-item:not(:last-child)
    .el-form-item__content,
  .my-container-main-container
    .demo-form-inline
    .el-form-item:not(:last-child)
    .el-form-item__content
    > * {
    width: 100% !important;
  }

  .my-container-main-container
    .demo-form-inline
    .el-form-item:last-child
    .el-form-item__content {
    justify-content: flex-end;
    width: 100%;
  }
}

@media (hover: none), (pointer: coarse) {
  .my-container-table-main .action-buttons.action-buttons {
    opacity: 1;
    transform: translateX(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .my-container-table-main .action-buttons.action-buttons {
    transform: translateX(0);
    transition: none;
  }
}

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

.my-container-table-main .el-table::before,
.my-container-table-main .el-table__inner-wrapper::before {
  background-color: rgb(0 0 0 / 3%);
}

.my-container-table-main .el-table__body tr:last-child > .el-table__cell {
  border-bottom-color: rgb(0 0 0 / 3%);
}

.my-container-main-container .el-divider--horizontal {
  margin: 0 !important;
}

.my-container-fullscreen:fullscreen {
  box-sizing: border-box;
  width: 100vw;
  height: 100vh;
  padding: 16px;
  background-color: var(--el-bg-color-page);
}

.my-container-main-container .demo-form-inline {
  display: flex;
  flex-wrap: wrap;
  gap: 12px 16px;
  align-items: center;
  padding: 12px 16px;
  margin: 0 !important;
}

.my-container-main-container .demo-form-inline.el-form--inline .el-form-item {
  margin-right: 0;
  margin-bottom: 0;
}

.my-container-main-container .demo-form-inline .el-form-item:last-child {
  margin-left: auto;
}

.my-container-main-container
  .demo-form-inline.no-action-align
  .el-form-item:last-child {
  margin-left: 0;
}

.my-container-main-container
  .demo-form-inline
  .el-form-item:last-child
  .el-form-item__content {
  gap: 8px;
}

.my-container-main-container .demo-form-inline .el-input,
.my-container-main-container .demo-form-inline .el-select,
.my-container-main-container .demo-form-inline .el-date-editor {
  max-width: 100%;
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

  display: flex;
  align-items: center;
  justify-content: flex-end;
  padding: 6px 12px 8px;
}

.my-container-table-footer .el-pagination .btn-next,
.my-container-table-footer .el-pagination .btn-prev {
  min-width: 56px;
}

.my-container-table-main .action-buttons {
  display: flex;
  gap: 8px; /* 按钮间距 */
  justify-content: center;
  opacity: 0.35;
  transform: translateX(2px);
  transition:
    opacity 0.18s ease,
    transform 0.18s ease;
}

.my-container-table-main
  .el-table__row:has(.el-button:focus-visible)
  .action-buttons,
.my-container-table-main .el-table__row:hover .action-buttons,
.my-container-table-main .el-table__row.current-row .action-buttons {
  opacity: 1;
  transform: translateX(0);
}
</style>
