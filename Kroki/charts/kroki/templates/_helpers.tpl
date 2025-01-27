{{/*
Expand the name of the chart.
*/}}
{{- define "kroki.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "kroki.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "kroki.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "kroki.labels" -}}
helm.sh/chart: {{ include "kroki.chart" . }}
{{ include "kroki.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "kroki.selectorLabels" -}}
app.kubernetes.io/name: {{ include "kroki.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}


{{- define "kroki.labelsWithName" -}}
helm.sh/chart: {{ include "kroki.chart" .scope }}
{{ include "kroki.selectorLabelsWithName" (dict "scope" .scope "name" .name) }}
{{- if .scope.Chart.AppVersion }}
app.kubernetes.io/version: {{ .scope.Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .scope.Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "kroki.selectorLabelsWithName" -}}
app.kubernetes.io/name: {{ include "kroki.name" .scope }}-{{ .name }}
app.kubernetes.io/instance: {{ .scope.Release.Name }}-{{ .name }}
{{- end }}
