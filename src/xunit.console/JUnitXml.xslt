<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output cdata-section-elements="message stack-trace"/>

  <xsl:template match="/">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="assemblies">
    <testsuites name="Test results">
      <xsl:attribute name="time">
        <xsl:value-of select="sum(assembly/@time)"/>
      </xsl:attribute>
      <xsl:attribute name="tests">
        <xsl:value-of select="sum(assembly/@total)"/>
      </xsl:attribute>
      <xsl:attribute name="failures">
        <xsl:value-of select="sum(assembly/@failed)"/>
      </xsl:attribute>
      <properties>
        <property name="skipped">
          <xsl:attribute name="value">
            <xsl:value-of select="sum(assembly/@skipped)"/>
          </xsl:attribute>
        </property>
        <property name="date">
          <xsl:attribute name="value">
            <xsl:value-of select="assembly[1]/@run-date"/>
          </xsl:attribute>
        </property>
        <property name="nunit-version">
          <xsl:attribute name="value">
            <xsl:value-of select="assembly[1]/@test-framework"/>
          </xsl:attribute>
        </property>
        <property name="clr-version">
          <xsl:attribute name="value">
            <xsl:value-of select="assembly[1]/@environment"/>
          </xsl:attribute>
        </property>
        <property name="date">
          <xsl:attribute name="value">
            <xsl:value-of select="assembly[1]/@run-date"/>
          </xsl:attribute>
        </property>
        <xsl:if test="sum(assembly/@failed) > 0">
          <property name="result" value="Failure" />
        </xsl:if>
        <xsl:if test="sum(assembly/@failed) = 0">
          <property name="result" value="Success" />
        </xsl:if>
      </properties>
      <xsl:apply-templates select="assembly/collection"/>
    </testsuites>
  </xsl:template>

  <xsl:template match="collection">
    <testsuite>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="time">
        <xsl:value-of select="@time"/>
      </xsl:attribute>
      <xsl:attribute name="tests">
        <xsl:value-of select="@total"/>
      </xsl:attribute>
      <xsl:attribute name="failures">
        <xsl:value-of select="@failed"/>
      </xsl:attribute>
      <xsl:apply-templates select="test"/>
    </testsuite>
  </xsl:template>

  <xsl:template match="test">
    <testcase>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:if test="@time">
        <xsl:attribute name="time">
          <xsl:value-of select="@time"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="failure"/>
    </testcase>
  </xsl:template>

  <xsl:template match="failure">
    <failure>
      <xsl:attribute name="type">
        <xsl:value-of select="@exception-type"/>
      </xsl:attribute>
      <xsl:if test="message">
        <xsl:attribute name="message">
          <xsl:value-of select="message"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="stack-trace">
        <xsl:value-of select="stack-trace"/>
      </xsl:if>
    </failure>
  </xsl:template>

</xsl:stylesheet>