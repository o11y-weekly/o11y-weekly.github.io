package com.demo.client;

import java.util.Arrays;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

import org.slf4j.MDC;

import ch.qos.logback.classic.spi.ILoggingEvent;
import ch.qos.logback.core.UnsynchronizedAppenderBase;
import io.opentelemetry.exporter.otlp.http.logs.OtlpHttpLogRecordExporter;
import io.opentelemetry.exporter.otlp.http.logs.OtlpHttpLogRecordExporterBuilder;
import io.opentelemetry.instrumentation.logback.appender.v1_0.internal.LoggingEventMapper;
import io.opentelemetry.sdk.OpenTelemetrySdk;
import io.opentelemetry.sdk.OpenTelemetrySdkBuilder;
import io.opentelemetry.sdk.logs.SdkLoggerProvider;
import io.opentelemetry.sdk.logs.SdkLoggerProviderBuilder;
import io.opentelemetry.sdk.logs.export.BatchLogRecordProcessor;
import io.opentelemetry.sdk.logs.export.BatchLogRecordProcessorBuilder;
import io.opentelemetry.sdk.resources.Resource;
import io.opentelemetry.sdk.resources.ResourceBuilder;

public class LogbackOpenTelemetryAppender extends UnsynchronizedAppenderBase<ILoggingEvent> {

  private static OpenTelemetrySdk sdk;
  private LoggingEventMapper mapper;
  
  private String endpoint = "http://localhost:4318/v1/logs"; // HTTP/protobuf only for now. Change OtlpHttpLogRecordExporter to OtlpGrpcLogRecordExporter, if you need gRPC (port 4317).
  private boolean captureExperimentalAttributes = false;
  private boolean captureCodeAttributes = false;
  private boolean captureMarkerAttribute = false;
  private boolean captureKeyValuePairAttributes = false;
  private List<String> captureMdcAttributes = Collections.emptyList();
  private Map<String, String> resourceAttributes = Collections.emptyMap();

  @Override
  public void start() {
    initializeOpenTelemetrySDK();
    initializeLogEventMapper();
    super.start();
  }

  @Override
  public void stop() {
    if (sdk != null) {
      sdk.close();
    }
    sdk = null;
    mapper = null;

    super.stop();
  }

  @Override
  protected void append(ILoggingEvent event) {
    mapper.emit(sdk.getSdkLoggerProvider(), event);
  }

  private void initializeLogEventMapper() {
    mapper = new LoggingEventMapper(
        captureExperimentalAttributes,
        captureMdcAttributes,
        captureCodeAttributes,
        captureMarkerAttribute,
        captureKeyValuePairAttributes);
  }

  private void initializeOpenTelemetrySDK() {

    // only instantiate SDK once.
    if (sdk != null) {
      return;
    }

    OpenTelemetrySdkBuilder sdkBuilder = OpenTelemetrySdk.builder();

    ResourceBuilder resourceBuilder = Resource.getDefault().toBuilder();
    for(Map.Entry<String, String> resourceAttribute : resourceAttributes.entrySet()) {
      resourceBuilder.put(resourceAttribute.getKey(), resourceAttribute.getValue());
    }
    

    OtlpHttpLogRecordExporterBuilder otlpHttpLogRecordExporterBuilder = OtlpHttpLogRecordExporter.builder();
    otlpHttpLogRecordExporterBuilder.setEndpoint(endpoint);

    BatchLogRecordProcessorBuilder batchLogRecordProcessorBuilder = BatchLogRecordProcessor.builder(otlpHttpLogRecordExporterBuilder.build());
    // LogRecordProcessor simpleLogRecordProcessor = SimpleLogRecordProcessor.create(otlpHttpLogRecordExporterBuilder.build());

    SdkLoggerProviderBuilder loggerProviderBuilder = SdkLoggerProvider.builder();
    loggerProviderBuilder.setResource(resourceBuilder.build());
    loggerProviderBuilder.addLogRecordProcessor(batchLogRecordProcessorBuilder.build());
    // loggerProviderBuilder.addLogRecordProcessor(simpleLogRecordProcessor);

    sdkBuilder.setLoggerProvider(loggerProviderBuilder.build());
    sdk = sdkBuilder.build();
  }

  /**
   * Sets whether experimental attributes should be set to logs. These attributes may be changed or removed in the future, so only enable this if you know you do not require attributes filled by this instrumentation to be stable across versions.
   */
  public void setCaptureExperimentalAttributes(boolean captureExperimentalAttributes) {
    this.captureExperimentalAttributes = captureExperimentalAttributes;
  }

  /**
   * Sets whether the code attributes (file name, class name, method name and line number) should be set to logs. Enabling these attributes can potentially impact performance (see https://logback.qos.ch/manual/layouts.html).
   *
   * @param captureCodeAttributes To enable or disable the code attributes (file name, class name, method name and line number)
   */
  public void setCaptureCodeAttributes(boolean captureCodeAttributes) {
    this.captureCodeAttributes = captureCodeAttributes;
  }

  /**
   * Sets whether the marker attribute should be set to logs.
   *
   * @param captureMarkerAttribute To enable or disable the marker attribute
   */
  public void setCaptureMarkerAttribute(boolean captureMarkerAttribute) {
    this.captureMarkerAttribute = captureMarkerAttribute;
  }

  /**
   * Sets whether the key value pair attributes should be set to logs.
   *
   * @param captureKeyValuePairAttributes To enable or disable the marker attribute
   */
  public void setCaptureKeyValuePairAttributes(boolean captureKeyValuePairAttributes) {
    this.captureKeyValuePairAttributes = captureKeyValuePairAttributes;
  }

  /** Configures the {@link MDC} attributes that will be copied to logs. */
  public void setCaptureMdcAttributes(String attributes) {
    if (attributes != null) {
      captureMdcAttributes = filterBlanksAndNulls(attributes.split(","));
    } else {
      captureMdcAttributes = Collections.emptyList();
    }
  }
  
  /** Configures the {@link MDC} attributes that will be copied to logs. */
  public void setResourceAttributes(String attributes) {
    if (attributes != null) {
      resourceAttributes = getResourceAttributesFromConfigurationString(attributes, ",");
    } else {
      resourceAttributes = Collections.emptyMap();
    }
  }
  
  public void setEndpoint(String endpoint) {
    this.endpoint = endpoint;
  }

  private Map<String, String> getResourceAttributesFromConfigurationString(String attributes, String separator) {
    Map<String, String> resourceAttributes = new HashMap<>();
    String[] keyValuePairs = attributes.split(separator);
    for(String pair : keyValuePairs) {
      String[] keyValuePair = pair.split("=");
      resourceAttributes.put(keyValuePair[0], keyValuePair[1]);
    }
    return resourceAttributes;
  }

  // copied from SDK's DefaultConfigProperties
  private static List<String> filterBlanksAndNulls(String[] values) {
    return Arrays.stream(values).map(String::trim).filter(s -> !s.isEmpty()).collect(Collectors.toList());
  }
}